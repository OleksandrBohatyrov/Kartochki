using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Kartochki
{
    public partial class MainPage : ContentPage
    {
        private const string FilePath = "dictionary.txt";
        private CarouselView carouselView;
        private bool isRotated = false;
        private List<Product> products;

        public MainPage()
        {
            carouselView = new CarouselView
            {
                VerticalOptions = LayoutOptions.Center,
            };

            LoadDictionaryFromFile(); // Загрузка словаря из файла

            carouselView.ItemTemplate = new DataTemplate(() =>
            {
                Frame frame = new Frame();
                frame.BindingContextChanged += (sender, e) => { isRotated = false; };

                Label header = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 24,
                    TextColor = Color.FromHex("#000000"),
                    Margin = 10,
                };
                header.SetBinding(Label.TextProperty, "Nimi");

                Label perevod = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#000000"),
                    VerticalOptions = LayoutOptions.Center,
                    Margin = 10,
                    FontSize = 20,
                    IsVisible = false // Initially invisible
                };
                perevod.SetBinding(Label.TextProperty, "Tõlkida");

                StackLayout st = new StackLayout() { Children = { header, perevod }, VerticalOptions = LayoutOptions.Center, WidthRequest = 300, HeightRequest = 300, BackgroundColor = Color.FromHex("#ccb6b6") };
                frame.Content = st;

                frame.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () => await FlipCard(frame, perevod)),
                });

                frame.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    NumberOfTapsRequired = 2,
                    Command = new Command(async () =>
                    {
                        var product = (Product)frame.BindingContext;
                        bool result = await DisplayAlert("Kinnitus", $"Kas olete kindel, et soovite kustutada sõna '{product.Name}'?", "Jah", "Ei");
                        if (result)
                        {
                            RemoveWordFromDictionary(product);
                            carouselView.ItemsSource = products;
                        }
                    })
                });

                return frame;
            });

            Button addButton = new Button
            {
                Text = "Lisa sõna",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 20),
            };
            addButton.Clicked += OnAddButtonClick;

            Content = new StackLayout
            {
                Children = { carouselView, addButton },
            };
        }

        private async void OnAddButtonClick(object sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("Lisa sõna", "Sisestage sõna:");
            string perevod = await DisplayPromptAsync("Lisa sõna", "Sisestage tõlge:");

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(perevod))
            {
                AddWordToDictionary(name, perevod);

                // Create a copy of VisibleViews
                List<View> visibleViewsCopy = new List<View>(carouselView.VisibleViews);

                // Flip back the card after adding word
                foreach (var item in visibleViewsCopy)
                {
                    Frame frame = (Frame)item;
                    Label perevodLabel = ((StackLayout)frame.Content).Children[1] as Label;
                    perevodLabel.IsVisible = false; // Hide translation label after adding word
                    await FlipCard(frame, perevodLabel);
                }
            }
            else
            {
                await DisplayAlert("Viga", "Palun täitke kõik väljad.", "OK");
            }
        }

        private async Task FlipCard(Frame frame, Label perevodLabel)
        {
            if (!isRotated)
            {
                await frame.RotateYTo(-90, 250); // Rotate the frame to flip it

                // Wait for a moment before rotating back to give a flip effect
                await Task.Delay(100);

                // Show translation label on flipped side
                perevodLabel.IsVisible = true;

                await frame.RotateYTo(0, 250); // Rotate back to original position
                isRotated = true;
            }
            else
            {
                await frame.RotateYTo(-90, 250);
                await Task.Delay(100);
                perevodLabel.IsVisible = false;
                await frame.RotateYTo(0, 250);
                isRotated = false;
            }
        }

        private void LoadDictionaryFromFile()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    products = new List<Product>();
                    string[] lines = File.ReadAllLines(FilePath);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length >= 2) 
                        {
                            products.Add(new Product { Name = parts[0], Perevod = parts[1] });
                        }
                    }
                }
                else
                {
                    products = new List<Product>(); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Viga sõnastiku laadimisel: {ex.Message}");
                products = new List<Product>(); 
            }
        }

        private void AddWordToDictionary(string name, string perevod)
        {
            products.Add(new Product { Name = name, Perevod = perevod });
            SaveDictionaryToFile();
            carouselView.ItemsSource = products;
        }

        private void RemoveWordFromDictionary(Product product)
        {
            if (products != null)
            {
                products.Remove(product);
                SaveDictionaryToFile();
                carouselView.ItemsSource = products;
            }
        }

        private void SaveDictionaryToFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(FilePath))
                {
                    foreach (Product product in products)
                    {
                        writer.WriteLine($"{product.Name},{product.Perevod}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Viga sõnastiku salvestamisel: {ex.Message}");
            }
        }
    }
}
