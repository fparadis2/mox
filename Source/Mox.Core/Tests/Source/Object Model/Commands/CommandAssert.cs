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

using Mox.Transactions;

namespace Mox
{
    partial class Assert
    {
        #region Methods

        /// <summary>
        /// Asserts that the given <paramref name="operation"/> produces X commands on the given <paramref name="controller"/>.
        /// </summary>
        public static void Produces(IObjectController controller, Action operation, int numCommands)
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();

            try
            {
                controller.CommandExecuted += sink;

                operation();

                AreEqual(numCommands, sink.TimesCalled, "Expected operation to produce {0} command but produced {1}", numCommands, sink.TimesCalled);
            }
            finally
            {
                controller.CommandExecuted -= sink;
            }
        }

        /// <summary>
        /// Asserts that the given <paramref name="operation"/> only creates one command on the given <paramref name="controller"/>.
        /// </summary>
        public static void IsAtomic(IObjectController controller, Action operation)
        {
            Produces(controller, operation, 1);
        }

        /// <summary>
        /// Asserts that the given <paramref name="action"/> is undoable/redoable by verifying an initial and a final state.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="initialVerification"></param>
        /// <param name="action"></param>
        /// <param name="finalVerification"></param>
        public static void IsUndoRedoable(IObjectController controller, Action initialVerification, Action action, Action finalVerification)
        {
            Assert.IsNotNull(controller);
            Assert.IsNotNull(initialVerification);
            Assert.IsNotNull(action);
            Assert.IsNotNull(finalVerification);

            const string Token = "CommandAssert";

            initialVerification();

            controller.BeginTransaction(Token);

            action();

            finalVerification();

            controller.EndTransaction(true, Token);

            initialVerification();
        }

        public static void Undo(IObjectController controller, Action action, Action<Action> undoScope)
        {
            const string Token = "CommandAssert";

            controller.BeginTransaction(Token);

            action();

            bool ended = false;
            undoScope(() =>
            {
                controller.EndTransaction(true, Token);
                ended = true;
            });
            Assert.That(ended);
        }

        #endregion
    }
}
