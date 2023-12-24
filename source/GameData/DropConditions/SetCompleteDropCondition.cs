using System;

namespace jshepler.ngu.mods.GameData.DropConditions
{
    internal class SetCompleteDropCondition : BaseDropCondition
    {
        private ItemSets _itemSet;
        private static ItemList il;

        internal SetCompleteDropCondition(ItemSets itemSet)
        {
            _itemSet = itemSet;
        }

        internal override bool IsConditionMet()
        {
            if (il == null)
                il = Plugin.Character.inventory.itemList;

            return _itemSet switch
            {
                ItemSets.Amalgamate => il.amalgamateComplete,
                ItemSets.AntiWaldo => il.antiWaldoComplete,
                ItemSets.BadlyDrawn => il.badlyDrawnComplete,
                ItemSets.Beardverse => il.beardverseComplete,
                ItemSets.Beast1 => il.beast1complete,
                ItemSets.Bread => il.breadverseComplete,
                ItemSets.Cave => il.caveComplete,
                ItemSets.Choco => il.chocoComplete,
                ItemSets.Clock => il.clockComplete,
                ItemSets.Construction => il.constructionComplete,
                ItemSets.Duck => il.duckComplete,
                ItemSets.Edgy => il.edgyComplete,
                ItemSets.EdgyBoots => il.edgyBootsComplete,
                ItemSets.Exile => il.exileComplete,
                ItemSets.Fad => il.fadComplete,
                ItemSets.Forest => il.forestComplete,
                ItemSets.Gaudy => il.gaudyComplete,
                ItemSets.Ghost => il.ghostComplete,
                ItemSets.Godmother => il.godmotherComplete,
                ItemSets.GRB => il.GRBComplete,
                ItemSets.Halloweenies => il.halloweeniesComplete,
                ItemSets.HSB => il.HSBComplete,
                ItemSets.Jake => il.jakeComplete,
                ItemSets.JRPG => il.jrpgComplete,
                ItemSets.Mega => il.megaComplete,
                ItemSets.Meta => il.metaComplete,
                ItemSets.Nerd => il.nerdComplete,
                ItemSets.Nether => il.nerdComplete,
                ItemSets.Party => il.partyComplete,
                ItemSets.Pirate => il.pirateComplete,
                ItemSets.Pretty => il.prettyComplete,
                ItemSets.Rad => il.radComplete,
                ItemSets.RockLobster => il.rockLobsterComplete,
                ItemSets.School => il.schoolComplete,
                ItemSets.Sewers => il.sewersComplete,
                ItemSets.Sky => il.skyComplete,
                ItemSets.Space => il.spaceComplete,
                ItemSets.Stealth => il.stealthComplete,
                ItemSets.That70s => il.that70sComplete,
                ItemSets.Training => il.trainingComplete,
                ItemSets.TwoD => il.twoDComplete,
                ItemSets.Typo => il.typoComplete,
                ItemSets.UUGsRings => il.uugRingComplete,
                ItemSets.Waldo => il.waldoComplete,
                ItemSets.Western => il.westernComplete,

                _ => false
            };
        }
    }
}
