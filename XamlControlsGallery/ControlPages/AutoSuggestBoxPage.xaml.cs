﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using AppUIBasics.Data;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AppUIBasics.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AutoSuggestBoxPage : Page
    {
        public AutoSuggestBoxPage()
        {
            this.InitializeComponent();
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<string> suggestions = new List<string>()
                {
                    sender.Text + "1",
                    sender.Text + "2"
                };
                Control1.ItemsSource = suggestions;
            }
        }
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            SuggestionOutput.Text = args.SelectedItem.ToString();
        }

        /// <summary>
        /// This event gets fired anytime the text in the TextBox gets updated.
        /// It is recommended to check the reason for the text changing by checking against args.Reason
        /// </summary>
        /// <param name="sender">The AutoSuggestBox whose text got changed.</param>
        /// <param name="args">The event arguments.</param>
        private void Control2_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            //We only want to get results when it was a user typing,
            //otherwise we assume the value got filled in by TextMemberPath
            //or the handler for SuggestionChosen
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suggestions = SearchControls(sender.Text);

                if (suggestions.Count > 0)
                    sender.ItemsSource = suggestions;
                else
                    sender.ItemsSource = new string[] { "No results found" };
            }
        }

        /// <summary>
        /// This event gets fired when:
        ///     * a user presses Enter while focus is in the TextBox
        ///     * a user clicks or tabs to and invokes the query button (defined using the QueryIcon API)
        ///     * a user presses selects (clicks/taps/presses Enter) a suggestion
        /// </summary>
        /// <param name="sender">The AutoSuggestBox that fired the event.</param>
        /// <param name="args">The args contain the QueryText, which is the text in the TextBox,
        /// and also ChosenSuggestion, which is only non-null when a user selects an item in the list.</param>
        private void Control2_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is ControlInfoDataItem)
            {
                //User selected an item, take an action
                SelectControl(args.ChosenSuggestion as ControlInfoDataItem);
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                //Do a fuzzy search based on the text
                var suggestions = SearchControls(sender.Text);
                if(suggestions.Count > 0)
                {
                    SelectControl(suggestions.FirstOrDefault());
                }
            }
        }

        /// <summary>
        /// This event gets fired as the user keys through the list, or taps on a suggestion.
        /// This allows you to change the text in the TextBox to reflect the item in the list.
        /// Alternatively you can use TextMemberPath.
        /// </summary>
        /// <param name="sender">The AutoSuggestBox that fired the event.</param>
        /// <param name="args">The args contain SelectedItem, which contains the data item of the item that is currently highlighted.</param>
        private void Control2_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            //Don't autocomplete the TextBox when we are showing "no results"
            if (args.SelectedItem is ControlInfoDataItem control)
            {
                sender.Text = control.Title;
            }
        }

        /// <summary>
        /// This
        /// </summary>
        /// <param name="contact"></param>
        private void SelectControl(ControlInfoDataItem control)
        {
            if (control != null)
            {
                ControlDetails.Visibility = Visibility.Visible;

                BitmapImage image = new BitmapImage(new Uri(control.ImagePath));
                ControlImage.Source = image;

                ControlTitle.Text = control.Title;
                ControlSubtitle.Text = control.Subtitle;
            }
        }

        private List<ControlInfoDataItem> SearchControls(string query)
        {
            var suggestions = new List<ControlInfoDataItem>();

            var querySplit = query.Split(" ");
            foreach (var group in ControlInfoDataSource.Instance.Groups)
            {
                var matchingItems = group.Items.Where(
                    item =>
                    {
                        // Idea: check for every word entered (separated by space) if it is in the name,  
                        // e.g. for query "split button" the only result should "SplitButton" since its the only query to contain "split" and "button" 
                        // If any of the sub tokens is not in the string, we ignore the item. So the search gets more precise with more words 
                        bool flag = item.IncludedInBuild;
                        foreach (string queryToken in querySplit)
                        {
                            // Check if token is not in string 
                            if (item.Title.IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) < 0)
                            {
                                // Token is not in string, so we ignore this item. 
                                flag = false;
                            }
                        }
                        return flag;
                    });
                foreach (var item in matchingItems)
                {
                    suggestions.Add(item);
                }
            }
            return suggestions.OrderByDescending(i => i.Title.StartsWith(query, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.Title).ToList();
        }
    }
}
