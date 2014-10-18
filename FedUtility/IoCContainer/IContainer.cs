#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 IContainer.cs is part of FedUtility.
 
 Utility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 Utility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with FedUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace FedUtility.IoCContainer
{
    #region

    using System;

    #endregion

    internal interface IContainer
    {
        #region Methods

        void Deregister(Type type, string instanceName = null);

        void Deregister<T>(string instanceName = null);
        bool IsRegistered(Type type, string instanceName = null);

        bool IsRegistered<T>(string instanceName = null);

        void Register(Type from, Type to, bool singleton, bool initialize, string instanceName = null);

        void Register<TFrom, TTo>(bool singleton, bool initialize, string instanceName = null) where TTo : TFrom;

        void Register(Type type, Func<object> createInstanceDelegate, bool singleton, bool initialize,
            string instanceName = null);

        void Register<T>(Func<T> createInstanceDelegate, bool singleton, bool initialize, string instanceName = null);

        object Resolve(Type type, string instanceName = null);

        T Resolve<T>(string instanceName = null);

        #endregion
    }
}