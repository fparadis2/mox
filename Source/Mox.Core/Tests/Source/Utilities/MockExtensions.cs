﻿// Copyright (c) François Paradis
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Rhino.Mocks;
using Rhino.Mocks.Impl;

namespace Mox
{
    /// <summary>
    /// Mocking utilities.
    /// </summary>
    public static class MockExtensions
    {
        [DebuggerStepThrough]
        public static void Test(this MockRepository mockery, Action test)
        {
            mockery.ReplayAll();
            test();
            mockery.VerifyAll();
            mockery.BackToRecordAll();
        }

        [DebuggerStepThrough]
        public static IDisposable Test(this MockRepository mockery)
        {
            mockery.ReplayAll();

            return new DisposableHelper(() =>
            {
                if (Marshal.GetExceptionCode() == 0)
                {
                    mockery.VerifyAll();
                    mockery.BackToRecordAll();
                }
            });
        }

        public static void LogExpectations(this MockRepository mockery)
        {
            RhinoMocks.Logger = new TraceWriterExpectationLogger();
        }
    }
}
