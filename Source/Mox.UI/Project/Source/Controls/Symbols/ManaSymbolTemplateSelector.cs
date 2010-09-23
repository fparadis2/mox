using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Mox.UI
{
    [ContentProperty("SymbolsMapping")]
    public class ManaSymbolTemplateSelector : DataTemplateSelector
    {
        #region Inner Types

        public class ManaSymbolToTemplateCollection : KeyedCollection<ManaSymbol, ManaSymbolTemplateSelectorMapping>
        {
            protected override ManaSymbol GetKeyForItem(ManaSymbolTemplateSelectorMapping mapping)
            {
                return mapping.ManaSymbol;
            }
        }

        #endregion

        #region Variables

        private readonly ManaSymbolToTemplateCollection m_symbolsMapping = new ManaSymbolToTemplateCollection();

        #endregion

        #region Properties

        public DataTemplate DefaultTemplate
        {
            get;
            set;
        }

        public ManaSymbolToTemplateCollection SymbolsMapping
        {
            get { return m_symbolsMapping; }
        }

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ManaSymbol)
            {
                ManaSymbol symbol = (ManaSymbol)item;
                if (m_symbolsMapping.Contains(symbol))
                {
                    ManaSymbolTemplateSelectorMapping mapping = m_symbolsMapping[symbol];
                    return mapping.Template;
                }
            }

            return DefaultTemplate;
        }

        #endregion
    }
}
