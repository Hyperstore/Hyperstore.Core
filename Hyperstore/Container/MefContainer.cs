// Copyright 2014 Zenasoft.  All rights reserved.
//
// This file is part of Hyperstore.
//
//    Hyperstore is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Hyperstore is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Hyperstore.  If not, see <http://www.gnu.org/licenses/>.

#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Events;
#endregion

namespace Hyperstore.Modeling
{
    //http://mef.codeplex.com/wikipage?title=MetroChanges
    // TODO a finir
    class MefContainer : ICompositionService, Hyperstore.Modeling.IMefContainer
    {
        public MefContainer(params Assembly[] assemblies)
        {
 //App.CompositionHost.SatisfyImports(this);
        }

    //internal static CompositionHost CompositionHost
    //{
    //    get
    //    {
    //        return _compositionHost ?? (_compositionHost = new ContainerConfiguration()
    //            .WithAssembly(System.Reflection.Assembly.GetExecutingAssembly())
    //            .CreateContainer());
    //    }
    //}
        public void Compose(params Assembly[] assemblies)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Lazy<ICommandInterceptor, ICommandInterceptorMetadata>> GetInterceptorsForDomainModel(IDomainModel domainModel)
        {
            yield break;
        }
        
        public IEnumerable<Lazy<IEventHandler, IEventHandlerMetadata>> GetEventHandlers()
        {
            yield break;
        }

        public IEnumerable<Lazy<ICommandHandler, ICommandHandlerMetadata>> GetCommandHandlersForDomainModel(IDomainModel domainModel)
        {
            yield break;
        }

        public void Dispose()
        {
        }
    }
}
