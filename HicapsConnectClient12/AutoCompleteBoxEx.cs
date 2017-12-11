using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace HicapsConnectClient12
{
    // this class is from a discussion forum:
    // http://wpf.codeplex.com/discussions/209974

    public class AutoCompleteBoxEx : System.Windows.Controls.AutoCompleteBox
    {
        private bool _selectorIsListBox = false;
        private bool _selectorSelectionChangedHandlerRegisterd = false;
        private const int SELECTOR_MAX_HEIGHT = 250;
        private TextBox _textBox;
        private ListBox _selector;

        public AutoCompleteBoxEx()
        {
            Loaded += AutoCompleteBoxExLoaded;
        }

        protected void AutoCompleteBoxExLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_selector == null)
            {
                _selector = Template.FindName("Selector", this) as ListBox;

                if (_selector != null)
                    _selectorIsListBox = true;
            }

            if (!_selectorSelectionChangedHandlerRegisterd && _selectorIsListBox && _selector != null)
            {
                _selector.SelectionChanged += ListBoxSelectionChanged;
                _selector.MaxHeight = SELECTOR_MAX_HEIGHT;
                _selectorSelectionChangedHandlerRegisterd = true;
            }
        }

        static void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox box = ((ListBox)sender);
            box.ScrollIntoView(box.SelectedItem);
            e.Handled = true;
        }
    }
}
