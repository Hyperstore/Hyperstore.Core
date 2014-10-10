using Hyperstore.Modeling;
using Hyperstore.Modeling.Scopes;
using Hyperstore.Tests.Mocks;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Tests.Scopes
{
    
    public class ScopesControlerTests
    {

        [Fact]
        public void Scopes_loadDomains()
        {
            IScopeManager<IDomainModel> scopes = new ScopeControler<IDomainModel>(null);

            var domain = new MockDomainModel("Test");

            scopes.RegisterScope(domain);
            Assert.Equal(0, scopes.GetScopes(ScopesSelector.Enabled).Count());
            Assert.Equal(0, scopes.GetScopes(ScopesSelector.Loaded).Count());
            Assert.Null(scopes.GetActiveScope("Test", 0));

            scopes.EnableScope(domain);
            Assert.Equal(1, scopes.GetScopes(ScopesSelector.Enabled).Count());
            Assert.Equal(1, scopes.GetScopes(ScopesSelector.Loaded).Count());
            Assert.NotNull(scopes.GetActiveScope("Test", 0));
        }

        [Fact]
        public void Scopes_activeDomain()
        {
            IScopeManager<IDomainModel> scopes = new ScopeControler<IDomainModel>(null);

            var domain = new MockDomainModel("Test");

            scopes.RegisterScope(domain);
            scopes.OnSessionCreated(null, 1);

            scopes.EnableScope(domain);

            Assert.Null(scopes.GetActiveScope("Test", 1)); // Valid after the current session
            Assert.Equal(0, scopes.GetScopes(ScopesSelector.Enabled, 1).Count());
            Assert.Equal(1, scopes.GetScopes(ScopesSelector.Loaded).Count());

            scopes.OnSessionCreated(null, 2);
            Assert.NotNull(scopes.GetActiveScope("Test", 2));
            Assert.Equal(1, scopes.GetScopes(ScopesSelector.Enabled, 2).Count());
            Assert.Equal(1, scopes.GetScopes(ScopesSelector.Loaded).Count());
        }

        [Fact]
        public void Scopes_unloadDomain()
        {
            IScopeManager<IDomainModel> scopes = new ScopeControler<IDomainModel>(null);

            var domain = new MockDomainModel("Test");

            scopes.RegisterScope(domain);
            scopes.OnSessionCreated(null, 2);
            scopes.EnableScope(domain); // domain enable from session > 2

            scopes.OnSessionCreated(null, 3);
            Assert.NotNull(scopes.GetActiveScope("Test", 3));
            Assert.Null(scopes.GetActiveScope("Test", 1));

            scopes.UnloadScope(domain);
            Assert.Equal(1, scopes.GetScopes(ScopesSelector.Enabled, 3).Count());
            Assert.Equal(0, scopes.GetScopes(ScopesSelector.Enabled, 2).Count());

            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(2);
            Assert.Equal(0, scopes.GetScopes(ScopesSelector.Enabled, 2).Count());

            Assert.Equal(1, scopes.GetScopes(ScopesSelector.Enabled, 3).Count());
            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(3);
            Assert.Equal(0, scopes.GetScopes(ScopesSelector.Enabled, 3).Count());
        }

        [Fact]
        public void Scopes_loadExtension()
        {
            IScopeManager<IDomainModel> scopes = new ScopeControler<IDomainModel>(null);

            var domain = new MockDomainModel("Test");
            var ext = new MockDomainModel("test", "ext");

            scopes.RegisterScope(domain);
            scopes.OnSessionCreated(null, 2);
            scopes.EnableScope(domain);

            scopes.OnSessionCreated(null, 3);
            Assert.Equal(domain, scopes.GetActiveScope("Test", 3));
            scopes.RegisterScope(ext);
            Assert.Equal(domain, scopes.GetActiveScope("Test", 3));
            scopes.EnableScope(ext);
            Assert.Equal(domain, scopes.GetActiveScope("Test", 3));
            scopes.OnSessionCreated(null, 4);

            Assert.Equal(ext, scopes.GetActiveScope("Test", 4));
            Assert.Equal(domain, scopes.GetActiveScope("Test", 3));

            Assert.Equal(1, scopes.GetScopes(ScopesSelector.Enabled, 4).Count());
            Assert.Equal(2, scopes.GetScopes(ScopesSelector.Loaded, 4).Count());
            scopes.UnloadScope(ext);

            Assert.Equal(ext, scopes.GetActiveScope("Test", 4));
            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(4);

            Assert.Equal(domain, scopes.GetActiveScope("Test", 3));

            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(2);
            ((ScopeControler<IDomainModel>)scopes).OnSessionCompleted(3);
        }
    }
}
