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
        #region Leonin Scimitar

        [Test]
        public void Test_Leonin_Scimitar()
        {
            Card creature1 = CreateCreatureOnBattlefield(m_playerA, 1, 1);
            Card creature2 = CreateCreatureOnBattlefield(m_playerA, 2, 3);

            Card card = InitializeCard("Leonin Scimitar");
            card.Zone = m_game.Zones.Battlefield;

            var equipAbility = card.Abilities.OfType<ActivatedAbility>().Single();

            // -- Can play a first time
            Assert.IsTrue(CanPlay(m_playerA, equipAbility));
            Expect_Target(m_playerA, creature1);
            Expect_PayManaCost(m_playerA, "1");
            PlayAndResolve(m_playerA, equipAbility);

            Assert.AreEqual(creature1, card.AttachedTo);
            Assert_PT(creature1, 2, 2);
            Assert_PT(creature2, 2, 3);

            // -- Can play a second time
            Assert.IsTrue(CanPlay(m_playerA, equipAbility));
            Expect_Target(m_playerA, creature2);
            Expect_PayManaCost(m_playerA, "1");
            PlayAndResolve(m_playerA, equipAbility);

            Assert.AreEqual(creature2, card.AttachedTo);
            Assert_PT(creature1, 1, 1);
            Assert_PT(creature2, 3, 4);
        }

        #endregion

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
