using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kartochki
{
    public partial class MainPage : ContentPage
    {
        private const string FilePath = "dictionary.txt";
        private CarouselView carouselView;

        public MainPage()
        {
            carouselView = new CarouselView
            {
                VerticalOptions = LayoutOptions.Center,
            };

            List<Product> products = LoadDictionaryFromFile();

            if (products == null)
            {
                products = new List<Product>
                {
                    new Product { Name = "Vihm", Perevod = "Дождь" },
                    new Product { Name = "Päike", Perevod= "Солнце" },
                    new Product { Name = "Maa", Perevod = "Земля" }
                };
            }

            carouselView.ItemsSource = products;

            carouselView.ItemTemplate = new DataTemplate(() =>
            {
                Label header = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 18,
                    TextColor = Color.FromHex("#000000"),
                    Margin = 10,
                };
                header.SetBinding(Label.TextProperty, "Name");

                Label perevod = new Label { HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromHex("#000000"), Margin = 10 };
                perevod.SetBinding(Label.TextProperty, "Perevod");

                StackLayout st = new StackLayout() { header, perevod };
                st.WidthRequest = 300;
                st.HeightRequest = 300;
                st.BackgroundColor = Color.FromHex("#ccb6b6");
                Frame frame = new Frame();
                frame.Content = st;
                return frame;
            });

            Button addButton = new Button
            {
                Text = "Добавить слово",
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

        private List<Product> LoadDictionaryFromFile()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    List<Product> products = new List<Product>();
                    string[] lines = File.ReadAllLines(FilePath);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length >= 2) // Проверяем, что есть хотя бы две части
                        {
                            products.Add(new Product { Name = parts[0], Perevod = parts[1] });
                        }
                    }
                    return products;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке словаря: {ex.Message}");
                return null;
            }
        }

        private void SaveDictionaryToFile(List<Product> products)
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
                Console.WriteLine($"Ошибка при сохранении словаря: {ex.Message}");
            }
        }

        private async void OnAddButtonClick(object sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("Добавить слово", "Введите слово:");
            string perevod = await DisplayPromptAsync("Добавить слово", "Введите перевод:");

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(perevod))
            {
                AddWordToDictionary(name, perevod);
            }
            else
            {
                await DisplayAlert("Ошибка", "Пожалуйста, заполните все поля.", "OK");
            }
        }
        private void AddWordToDictionary(string name, string perevod)
        {
            List<Product> products = LoadDictionaryFromFile() ?? new List<Product>();
            products.Add(new Product { Name = name, Perevod = perevod });
            SaveDictionaryToFile(products);


            if (carouselView.ItemsSource == null)
            {
                carouselView.ItemsSource = products;
            }
            else
            {
                List<Product> updatedProducts = carouselView.ItemsSource as List<Product>;
                updatedProducts.Add(new Product { Name = name, Perevod = perevod });
                carouselView.ItemsSource = updatedProducts;
            }
        }



    }


}
