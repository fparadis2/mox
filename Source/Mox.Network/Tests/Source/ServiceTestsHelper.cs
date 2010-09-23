// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;

using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Interfaces;

namespace Mox.Network
{
    public class ServiceTestsHelper
    {
        #region Variables

        protected MockRepository m_mockery;

        protected IOperationContext m_operationContext;
        protected IMainService m_mainService;
        protected IServiceManager m_serviceManager;

        #endregion

        #region Setup

        public virtual void Setup()
        {
            m_mockery = new MockRepository();

            m_operationContext = m_mockery.StrictMock<IOperationContext>();
            m_mainService = m_mockery.StrictMock<IMainService>();
            m_serviceManager = m_mockery.StrictMock<IServiceManager>();            

            SetBasicExpectations();
        }

        protected void SetBasicExpectations()
        {
            SetupResult.For(m_serviceManager.OperationContext).Return(m_operationContext);
            SetupResult.For(m_serviceManager.MainService).Return(m_mainService);
        }

        #endregion

        #region Utilities

        protected IMethodOptions<T> Expect_GetCallbackChannel<T>(T channel)
        {
            return Expect.Call(m_operationContext.GetCallbackChannel<T>()).Return(channel);
        }

        protected IMethodOptions<string> Expect_GetSessionId(string sessionId)
        {
            return Expect.Call(m_operationContext.SessionId).Return(sessionId);
        }

        protected void Expect_Log(LogImportance importance, string messagePart)
        {
            Expect_Log(importance, messagePart, null);
        }

        protected void Expect_Log(LogImportance importance, string messagePart, string code)
        {
            Expect_Log(importance, messagePart, code, null);
        }

        protected void Expect_Log(LogImportance importance, string messagePart, string code, string subCategory)
        {
            m_serviceManager.Log(new LogMessage());

            LastCall.IgnoreArguments().Callback<LogMessage>(message =>
            {
                Assert.AreEqual(importance, message.Importance);

                if (!string.IsNullOrEmpty(code))
                {
                    Assert.AreEqual(code, message.Code);
                }

                if (!string.IsNullOrEmpty(subCategory))
                {
                    Assert.AreEqual(subCategory, message.SubCategory);
                }

                if (!string.IsNullOrEmpty(messagePart))
                {
                    Assert.IsTrue(message.Text.IndexOf(messagePart, StringComparison.OrdinalIgnoreCase) >= 0, "Expected {0} to be in {1}", messagePart, message);
                }

                return true;
            });
        }

        #endregion
    }
}
