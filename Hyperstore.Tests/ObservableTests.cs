using System;
using Xunit;
using Hyperstore.Modeling;
using Hyperstore.Tests.Model;
using System.Threading.Tasks;
using Hyperstore.Modeling.Domain;

namespace Hyperstore.Tests
{
    
    public class ObservableTests
    {
        [Fact]
        public async Task ObservableCollectionTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New()
                                    .UsingIdGenerator(r => new LongIdGenerator())
                                    .CreateAsync("test");

            Library lib;
            using (var session = domain.Store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib";
                session.AcceptChanges();
            }

            int added = 0;
            int removed = 0;
            ((System.Collections.Specialized.INotifyCollectionChanged)lib.BooksA).CollectionChanged += (sender, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    removed++;
                else
                    added++;
            };

            Book book2;
            Book book1;
            using (var session = domain.Store.BeginSession())
            {
                book1 = new Book(domain); // Added
                book1.Title = "aaa";
                lib.Books.Add(book1);

                var b = new Book(domain); // Ignored
                b.Title = "bbb";
                lib.Books.Add(b);

                book2 = new Book(domain); // Add
                book2.Title = "abcd";
                lib.Books.Add(book2);

                session.AcceptChanges();
            }

            using (var session = domain.Store.BeginSession())
            {
                lib.Books.Remove(book2); // Removed
                book1.Title = "zzz";     // Removed
                session.AcceptChanges();
            }

            Assert.Equal(2, added);
            Assert.Equal(2, removed);
        }

        [Fact]
        public async Task ObservableCollectionMultiChangesTest()
        {
            var store = await StoreBuilder.New().CreateAsync();
            await store.Schemas.New<LibraryDefinition>().CreateAsync();
            var domain = await store.DomainModels.New()
                                    .UsingIdGenerator(r => new LongIdGenerator())
                                    .CreateAsync("test");

            Library lib;
            using (var session = domain.Store.BeginSession())
            {
                lib = new Library(domain);
                lib.Name = "Lib";
                session.AcceptChanges();
            }

            int added = 0;
            int removed = 0;
            ((System.Collections.Specialized.INotifyCollectionChanged)lib.BooksA).CollectionChanged += (sender, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    removed++;
                else
                    added++;
            };

            Book book1;
            using (var session = domain.Store.BeginSession())
            {
                book1 = new Book(domain); // Added
                book1.Title = "aaa";
                lib.Books.Add(book1);

                session.AcceptChanges();
            }

            using (var session = domain.Store.BeginSession())
            {
                book1.Title = "zzz";     // Removed
                book1.Title = "abc";
                session.AcceptChanges();
            }

            Assert.Equal(1, added);
            Assert.Equal(0, removed);
        }
    }
}
