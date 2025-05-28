using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace SoundSystemSolutionTest
{
    public partial class Dash : Window
    {
        private Panel? _tab1Content;
        private Panel? _tab2Content;
        private Panel? _tab3Content;
        private ContentControl? _contentArea;
        private Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>();

        public Dash()
        {
            InitializeComponent();
            InitializeTabContent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            _contentArea = this.FindControl<ContentControl>("ContentArea");
            
            
            //////
            ////// WELCOME TAB CONTENT ///////
            //////
            _tab1Content = new Panel();
            
            var welcomePanel = new StackPanel
            {
                Spacing = 20,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            
            welcomePanel.Children.Add(new TextBlock
            {
                Text = "Welcome to Bicol Sound",
                FontSize = 28,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.DarkBlue),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });
            
            var border = new Border
            {
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.SkyBlue),
                BorderThickness = new Avalonia.Thickness(2),
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(20),
                Margin = new Avalonia.Thickness(0, 10, 0, 10),
                Background = new Avalonia.Media.SolidColorBrush(
                    Avalonia.Media.Color.FromArgb(20, 135, 206, 250))
            };
            
            var messagePanel = new StackPanel { Spacing = 15 };
            messagePanel.Children.Add(new TextBlock
            {
                Text = "Navigate through the tabs to explore different features.",
                FontSize = 18,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.DarkSlateGray)
            });
            
            border.Child = messagePanel;
            welcomePanel.Children.Add(border);
            
            _tab1Content.Children.Add(welcomePanel);
            
            //////
            ////// WELCOME TAB CONTENT END //////
            //////
            
            
            /*
            _tab2Content = new Panel
            {
                Children = { new TextBlock { Text = "Content 2", FontSize = 20, 
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, 
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center } }
            };
            */
            
            //////
            ////// TRANSACTION TAB CONTENT ///////
            //////
            
            _tab2Content = new Panel();

            var transactionPanel = new StackPanel
            {
                Spacing = 15,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
            };

            var transactionGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto)
                }
            };

            var transactionTitle = new TextBlock
            {
                Text = "Transaction History",
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#330d69")),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            transactionTitle.SetValue(Grid.RowProperty, 0);
            transactionTitle.SetValue(Grid.ColumnProperty, 0);
            transactionGrid.Children.Add(transactionTitle);

            var transactionButtonNew = new Button
            {
                Content = "New Transaction",
                FontSize = 15,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            transactionButtonNew.SetValue(Grid.RowProperty, 0);
            transactionButtonNew.SetValue(Grid.ColumnProperty, 1);
            transactionGrid.Children.Add(transactionButtonNew);
            
            transactionPanel.Children.Add(transactionGrid);
            _tab2Content.Children.Add(transactionPanel);
            
            //////
            ////// TRANSACTION TAB END ///////
            //////
            
            
            _tab3Content = new Panel
            {
                Children = { new TextBlock { Text = "Content 3", FontSize = 20, 
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, 
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center } }
            };
            
            //applies button names from .axaml counterpart
            var tab1Button = this.FindControl<Button>("Tab1Button");
            var tab2Button = this.FindControl<Button>("Tab2Button");
            var tab3Button = this.FindControl<Button>("Tab3Button");
            
            if (tab1Button != null) _tabButtons["Tab1Button"] = tab1Button;
            if (tab2Button != null) _tabButtons["Tab2Button"] = tab2Button;
            if (tab3Button != null) _tabButtons["Tab3Button"] = tab3Button;
        }

        private void InitializeTabContent()
        {
            if (_contentArea != null)
            {
                _contentArea.Content = _tab1Content;
            }
        }

        public void OnTabButtonClick(object sender, RoutedEventArgs args)
        {
            if (sender is Button clickedButton)
            {
                //updates the appearance of the clicked tab button
                foreach (var button in _tabButtons.Values)
                {
                    button.Classes.Remove("Selected");
                }
                
                clickedButton.Classes.Add("Selected");
                
                
                //changes the content of the content area based on which tab is clicked
                if (_contentArea != null)
                {
                    if (clickedButton.Name == "Tab1Button")
                        _contentArea.Content = _tab1Content;
                    else if (clickedButton.Name == "Tab2Button")
                        _contentArea.Content = _tab2Content;
                    else if (clickedButton.Name == "Tab3Button")
                        _contentArea.Content = _tab3Content;
                }
            }
        }
    }
}