using Hyperstore.Modeling;
using Hyperstore.Modeling.Scopes;
using Hyperstore.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Tests.Scopes
{
    [TestClass]
    public class ScopesControlerTests
    {

        [TestMethod]
        public void Scopes_loadDomains()
        {
            IScopeManager<IDomainModel> scopes = new ScopeControler<IDomainModel>(null);

            var domain = new MockDomainModel("Test");

            scopes.RegisterScope(domain);
            Assert.AreEqual(0, scopes.GetScopes(ScopesSelector.Enabled).Count());
            Assert.AreEqual(0, scopes.GetScopes(ScopesSelector.Loaded).Count());
            Assert.IsNull(scopes.GetActiveScope("Test", 0));

            scopes.EnableScope(domain);
            Assert.AreEqual(1, scopes.GetScopes(ScopesSelector.Enabled).Count());
            Assert.AreEqual(1, scopes.GetScopes(ScopesSelector.Loaded).Count());
            Assert.IsNotNull(scopes.GetActiveScope("Test", 0));
        }

        [TestMethod]
        public void Scopes_activeDomain()
        {
            IScopeManager<IDomainModel> scopes = new ScopeControler<IDomainModel>(null);

            var domain = new MockDomainModel("Test");

            scopes.RegisterScope(domain);
            scopes.OnSessionCreated(null, 1);

            scopes.EnableScope(domain);

            Assert.IsNull(scopes.GetActiveScope("Test", 1)); // Valid after the current session
            Assert.AreEqual(0, scopes.GetScopes(ScopesSelector.Enabled).Count(), 1);
            Assert.AreEqual(1, scopes.GetScopes(ScopesSelector.Loaded).Count());

            scopes.OnSessionCreated(null, 2);
            Assert.IsNotNull(scopes.GetActiveScope("Test", 2));
            Assert.AreEqual(1, scopes.GetScopes(ScopesSelector.Enabled).Count(), 2);
            Assert.AreEqual(1, scopes.GetScopes(ScopesSelector.Loaded).Count());
        }

        [TestMethod]
        public void Scopes_unloadDomain()
        {
            IScopeManager<IDomainModel> scopes = new ScopeControler<IDomainModel>(null);

            var domain = new MockDomainModel("Test");

            scopes.RegisterScope(domain);
            scopes.OnSessionCreated(null, 2);
            scopes.EnableScope(domain);

            scopes.OnSessionCreated(null, 3);
            Assert.IsNotNull(scopes.GetActiveScope("Test", 3));
            Assert.IsNull(scopes.GetActiveScope("Test", 1));

            scopes.UnloadScope(domain);
            Assert.AreEqual(0, scopes.GetScopes(ScopesSelector.Enabled).Count(), 3);
            Assert.AreEqual(1, scopes.GetScopes(ScopesSelector.Enabled).Count(), 2);

            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(2);
            Assert.AreEqual(0, scopes.GetScopes(ScopesSelector.Enabled).Count(), 2);

            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(3);


        }

        [TestMethod]
        public void Scopes_loadExtension()
        {
            IScopeManager<IDomainModel> scopes = new ScopeControler<IDomainModel>(null);

            var domain = new MockDomainModel("Test");
            var ext = new MockDomainModel("test", "ext");

            scopes.RegisterScope(domain);
            scopes.OnSessionCreated(null, 2);
            scopes.EnableScope(domain);

            scopes.OnSessionCreated(null, 3);
            Assert.AreEqual(domain, scopes.GetActiveScope("Test", 3));
            scopes.RegisterScope(ext);
            Assert.AreEqual(domain, scopes.GetActiveScope("Test", 3));
            scopes.EnableScope(ext);
            Assert.AreEqual(domain, scopes.GetActiveScope("Test", 3));
            scopes.OnSessionCreated(null, 4);

            Assert.AreEqual(ext, scopes.GetActiveScope("Test", 4));
            Assert.AreEqual(domain, scopes.GetActiveScope("Test", 3));

            Assert.AreEqual(1, scopes.GetScopes(ScopesSelector.Enabled).Count(), 4);
            Assert.AreEqual(2, scopes.GetScopes(ScopesSelector.Loaded).Count(), 4);
            scopes.UnloadScope(ext);

            Assert.AreEqual(ext, scopes.GetActiveScope("Test", 4));
            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(4);

            Assert.AreEqual(domain, scopes.GetActiveScope("Test", 3));

            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(2);
            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(3);
        }
    }
}
