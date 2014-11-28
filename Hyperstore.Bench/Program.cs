using Hyperstore.Modeling;
using Hyperstore.Modeling.Serialization;
using Hyperstore.Tests.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Bench
{
    class Program
    {
        private IHyperstore store;
        private ConcurrentDictionary<int, Identity> ids;
        private ISchema<TestDomainDefinition> schema;

        static void Main(string[] args)
        {
            var p = new Program();
            p.SerializatonBench(1000).Wait();
            //for (; ; )
            //{
            //    var p = new Program();
            //    p.BenchWithConstraints(cx).Wait();
            //    if (Console.ReadKey().Key == ConsoleKey.Escape)
            //        break;
            //    cx++;
            //}
        }

        public async Task SerializatonBench(int cx)
        {
            store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();

            var domain = await store.DomainModels
                                    .New()
                                        .UsingIdGenerator(services => new Hyperstore.Modeling.Domain.LongIdGenerator())
                                    .CreateAsync("Test");

            var sw = new Stopwatch();

            Console.WriteLine("Benchmark serializing {0} elements...", cx*2);

            Library lib;
            using (var session = store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib1";
                session.AcceptChanges();
            }

            Parallel.For(0, cx, (i) =>
            {
                using (var session = store.BeginSession())
                {
                    var b = new Book(domain);
                    b.Title = "Book \"book\" " + i.ToString();
                    b.Copies = i + 1;
                    lib.Books.Add(b);

                    var m = new Member(domain);
                    m.Name = "Book " + i.ToString();
                    lib.Members.Add(m);
                    session.AcceptChanges();
                }
            });
          //  Console.ReadKey();
            //Console.Write("xml ...");
            //sw.Start();
            //using (var stream = File.Open("test.xml",FileMode.Create))
            //{
            //    HyperstoreSerializer.Serialize(stream, domain);
            //}
            //Console.WriteLine(" : serialize {1:n}bytes in {0}ms ", sw.ElapsedMilliseconds, new FileInfo("test.xml").Length);

            //Console.Write("json ...");
            sw.Restart();
            using (var stream = File.Open("test.json", FileMode.Create))
            {
                HyperstoreSerializer.Serialize(stream, domain, new SerializationSettings { Options = SerializationOptions.Json | SerializationOptions.CompressSchema });
            }
           // Console.WriteLine(" : serialize {1:n}bytes in {0}ms ", sw.ElapsedMilliseconds, new FileInfo("test.json").Length);

            //Console.Write("old xml ...");
            //sw.Restart();
            //using (var stream = File.Open("test2.xml", FileMode.Create))
            //{
            //    var ser = new XmlDomainModelSerializer();
            //    await ser.Serialize(domain, stream, XmlSerializationOptions.Elements);
            //}
            //Console.WriteLine(" : serialize {1:n}bytes in {0}ms ", sw.ElapsedMilliseconds, new FileInfo("test2.xml").Length);


          //  Console.ReadKey();
        }

        public async Task BenchWithConstraints(int cx)
        {
            var collects = Enumerable.Range(0, 3).Select(i => GC.CollectionCount(i)).ToArray();
            
            long nb = 0;
            store = await StoreBuilder.New().CreateAsync();
            schema = await store.Schemas.New<TestDomainDefinition>().CreateAsync();

            var domain = await store.DomainModels
                                    .New()
                                        .UsingIdGenerator(services => new Hyperstore.Modeling.Domain.LongIdGenerator())
                                    .CreateAsync("Test");

            var sw = new Stopwatch();

            var mx = 10000;
            Console.WriteLine("Benchmark manipulating {0} elements...", mx);

            // Adding 100 constraints on each element
            var nbc = 100;
            if (cx % 2 == 1)
            {
                Console.WriteLine("Adding 100 implicit constraints on each element.");

                for (int i = 0; i < nbc; i++)
                    schema.Definition.XExtendsBaseClass.AddImplicitConstraint(self =>
                        System.Threading.Interlocked.Increment(ref nb) > 0,
                        "OK").Register();
            }

            Console.WriteLine("Running...");
            sw.Start();
            AddElement(domain, mx);
            Console.WriteLine("Added in {0}ms ", sw.ElapsedMilliseconds);
            sw.Restart();
            UpdateElement(mx);
            Console.WriteLine("Updated in {0}ms ", sw.ElapsedMilliseconds);
            sw.Restart();
            ReadElement(mx);
            Console.WriteLine("Read in {0}ms ", sw.ElapsedMilliseconds);
            sw.Restart();
            RemoveElement(mx);
            Console.WriteLine("Removed in {0}ms ", sw.ElapsedMilliseconds);
            sw.Restart();
            sw.Stop();

            Console.WriteLine("Expected {0} Value {1}", mx * nbc * 2, nb);
            domain = null;
            store.Dispose();
            ids.Clear();


            for (int i = 0; i < 3; i++)
                Console.WriteLine("GC collection {0} : {1}", i, GC.CollectionCount(i) - collects[i]);
            //Console.WriteLine();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            //Assert.AreEqual(mx * nbc * 2, nb); // Nbre de fois la contrainte est appelée (sur le add et le update)
            //Assert.IsTrue(sw.ElapsedMilliseconds < 3000, String.Format("ElapsedTime = {0}", sw.ElapsedMilliseconds));
        }

        private void AddElement(IDomainModel domain, int max)
        {
            ids = new ConcurrentDictionary<int, Identity>();

            Parallel.For(0, max, i =>
            {
                using (var tx = store.BeginSession(new SessionConfiguration { Mode = SessionMode.SkipNotifications }))
                {
                    var a = new XExtendsBaseClass(domain);
                    if (ids.TryAdd(i, ((IModelElement)a).Id))
                        tx.AcceptChanges();
                }
            });
        }

        private void UpdateElement(int max)
        {
            Parallel.For(0, max, i =>
            {
                using (var tx = store.BeginSession())
                {
                    var a = store.GetElement<XExtendsBaseClass>(ids[i]);
                    a.Name = "Toto" + i;
                    tx.AcceptChanges();
                }
            });
        }

        private void ReadElement(int max)
        {

            //Parallel.For(0, max, i =>
            for (int i = 0; i < max; i++)
            {
                using (var tx = store.BeginSession(new SessionConfiguration { Mode = SessionMode.SkipConstraints | SessionMode.SkipNotifications, Readonly = true }))
                {
                    var a = store.GetElement(ids[i]) as XExtendsBaseClass;
                    // var x = a.Name;
                    //  tx.AcceptChanges();
                }
            }
            //);
        }

        private int RemoveElement(int max)
        {
            Parallel.For(0, max, i =>
            {
                using (var tx = store.BeginSession())
                {
                    IModelElement a = store.GetElement<XExtendsBaseClass>(ids[i]);
                    //if (a != null)
                    {
                        Identity id;
                        if (!ids.TryRemove(i, out id) || id != ((IModelElement)a).Id)
                            throw new Exception();
                        a.Remove();
                    }
                    tx.AcceptChanges();
                }
            }
            );

            var x = store.GetElements(schema.Definition.XExtendsBaseClass).Count();
            var y = ids.Count();

            return x + y;
        }
    }
}