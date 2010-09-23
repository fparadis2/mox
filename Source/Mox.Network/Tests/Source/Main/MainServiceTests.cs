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
using System.ServiceModel;

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Network
{
    [TestFixture]
    public class MainServiceTests : ServiceTestsHelper
    {
        #region Variables

        private MainService m_service;
        private IMoxClient m_client1;
        private IMoxClient m_client2;

        #endregion

        #region Setup

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_service = new MainService(m_operationContext);

            m_client1 = m_mockery.StrictMultiMock<IMoxClient>(typeof(ICommunicationObject));
            m_client2 = m_mockery.StrictMultiMock<IMoxClient>(typeof(ICommunicationObject));
        }

        #endregion

        #region Utilities

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new MainService(null); });
        }

        #region Login

        [Test]
        public void Test_Login_returns_a_client()
        {
            Expect_GetSessionId("Session").Repeat.AtLeastOnce();
            Expect_GetCallbackChannel(m_client1);

            m_mockery.Test(delegate
            {
                LoginDetails details = m_service.Login("MyName");
                Assert.AreEqual(LoginResult.Success, details.Result);
                Assert.AreEqual("MyName", details.Client.Name);                
            });
        }

        [Test]
        public void Test_Login_in_a_second_time_does_nothing_and_returns_the_same_client()
        {
            Expect_GetSessionId("Session").Repeat.AtLeastOnce();
            Expect_GetCallbackChannel(m_client1).Repeat.AtLeastOnce();

            m_mockery.Test(delegate
            {
                LoginDetails originalDetails = m_service.Login("MyName");
                LoginDetails details = m_service.Login("MyName");

                Assert.AreEqual(LoginResult.AlreadyLoggedIn, details.Result);
                Assert.AreEqual(originalDetails.Client, details.Client);
            });
        }

        [Test]
        public void Test_Logout_does_nothing_if_not_already_logged_out()
        {
            Expect_GetSessionId("Session");

            m_mockery.Test(delegate
            {
                m_service.Logout();
            });
        }

        [Test]
        public void Test_Can_Login_logout_and_login_again()
        {
            Expect_GetSessionId("Session").Repeat.AtLeastOnce();
            Expect_GetCallbackChannel(m_client1).Repeat.Twice();

            m_mockery.Test(delegate
            {
                Assert.AreEqual(LoginResult.Success, m_service.Login("MyName").Result);
                m_service.Logout();
                Assert.AreEqual(LoginResult.Success, m_service.Login("MyName").Result);
            });
        }

        [Test]
        public void Test_GetClient_returns_null_if_no_client_associated_with_the_given_session_id()
        {
            m_mockery.Test(delegate
            {
                Assert.IsNull(m_service.GetClient("Invalid"));
                Assert.IsNull(m_service.GetClient(null));
                Assert.IsNull(m_service.GetClient(string.Empty));
            });
        }

        [Test]
        public void Test_GetClient_returns_logged_in_clients()
        {
            Expect_GetSessionId("Session").Repeat.AtLeastOnce();
            Expect_GetCallbackChannel(m_client1);

            m_mockery.Test(delegate
            {
                LoginDetails details = m_service.Login("MyName");
                Assert.AreEqual(details.Client, m_service.GetClient("Session"));

                m_service.Logout();
                Assert.IsNull(m_service.GetClient("Session"));
            });
        }

        #endregion

        #endregion
    }
}
