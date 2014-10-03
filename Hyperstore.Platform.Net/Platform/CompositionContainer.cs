//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
//
//		This file is part of Hyperstore (http://www.hyperstore.org)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;

namespace Hyperstore.Modeling.Platform.Net
{
    class CompositionService : ICompositionService 
    {
        [ImportMany]
        private IEnumerable<Lazy<Commands.ICommandInterceptor, ICommandInterceptorMetadata>> _commands=null;
        [ImportMany]
        private IEnumerable<Lazy<Events.IEventHandler, ICompositionMetadata>> _events = null;
        [ImportMany]
        private IEnumerable<Lazy<Commands.ICommandHandler, ICompositionMetadata>> _handlers = null;
        [ImportMany]
        private IEnumerable<Lazy<Metadata.Constraints.IConstraint, ICompositionMetadata>> _constraints = null;

        private CompositionContainer _container;

        public void Compose(params System.Reflection.Assembly[] assemblies)
        {
            var catalogs = new List<System.ComponentModel.Composition.Primitives.ComposablePartCatalog>();
            if (assemblies.Length == 0)
            {
                catalogs.Add(new DirectoryCatalog(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));
            }
            else
            {
                catalogs.AddRange(assemblies.Select(a => new AssemblyCatalog(a)));
            }

            _container = new CompositionContainer(new AggregateCatalog(catalogs));
            _container.SatisfyImportsOnce(this);
        }

        public IEnumerable<Lazy<Events.IEventHandler, ICompositionMetadata>> GetEventHandlers()
        {
            return _events;
        }

        public IEnumerable<Lazy<Commands.ICommandInterceptor, ICommandInterceptorMetadata>> GetInterceptorsForDomainModel(IDomainModel domainModel)
        {
            return _commands.Where(c => c.Metadata.DomainModel == null || String.Compare(domainModel.Name, c.Metadata.DomainModel, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public IEnumerable<Lazy<Commands.ICommandHandler, ICompositionMetadata>> GetCommandHandlersForDomainModel(IDomainModel domainModel)
        {
            return _handlers.Where(c => c.Metadata.DomainModel == null || String.Compare(domainModel.Name, c.Metadata.DomainModel, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public IEnumerable<Lazy<Metadata.Constraints.IConstraint, ICompositionMetadata>> GetConstraintsForDomainModel(IDomainModel domainModel)
        {
            return _constraints.Where(c => c.Metadata.DomainModel == null || String.Compare(domainModel.Name, c.Metadata.DomainModel, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
