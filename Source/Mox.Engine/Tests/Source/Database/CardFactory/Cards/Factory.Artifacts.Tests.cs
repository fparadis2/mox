using Mox.Abilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class FactoryArtifactTests : BaseFactoryTests
    {
        #region Phyrexian Vault

        [Test]
        public void Test_Phyrexian_Vault()
        {
            Card artifact = InitializeCard("Phyrexian Vault");
            artifact.Zone = m_game.Zones.Battlefield;

            Card sacrificed = CreateCard(m_playerA);
            sacrificed.Type = Type.Creature;
            sacrificed.Toughness = 1;
            sacrificed.Zone = m_game.Zones.Battlefield;

            ActivatedAbility tapAbility = artifact.Abilities.OfType<ActivatedAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, tapAbility));
            Expect_Target(m_playerA, sacrificed, TargetContextType.SacrificeCost);
            Expect_PayManaCost(m_playerA, "2");
            PlayAndResolve(m_playerA, tapAbility);

            Assert.That(artifact.Tapped);
            Assert.AreEqual(m_game.Zones.Graveyard, sacrificed.Zone);
        }

        #endregion
    }
}
