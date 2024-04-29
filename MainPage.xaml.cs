using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Kartochki
{
    public partial class MainPage : ContentPage
    {
        private const string FilePath = "dictionary.txt"; // Путь к файлу

        public MainPage()
        {
            carouselView = new CarouselView
            {
                VerticalOptions = LayoutOptions.Center,
            };

            // Проверяем наличие файла словаря и загружаем его, если существует
            List<Product> products = LoadDictionaryFromFile();

            // Если файла не существует или он пустой, создаем новый список
            if (products == null)
            {
                products = new List<Product>
                {
                    new Product { Name = "Vihm", Perevod = "Дождь", Image = "dotnet_bot.svg" },
                    new Product { Name = "Päike", Perevod= "Солнце", Image = "dotnet_bot.svg" },
                    new Product { Name = "Maa", Perevod = "Земля", Image = "dotnet_bot.svg" }
                };
            }

            carouselView.ItemsSource = products;

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

                Image image = new Image { WidthRequest = 150, HeightRequest = 150 };
                image.SetBinding(Image.SourceProperty, "Image");

                Label perevod = new Label { HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromHex("#000000"), Margin = 10 };
                perevod.SetBinding(Label.TextProperty, "Perevod");

                StackLayout st = new StackLayout() { header, image, perevod };
                st.WidthRequest = 300;
                st.HeightRequest = 300;
                st.BackgroundColor = Color.FromHex("#ccb6b6");
                Frame frame = new Frame();
                frame.Content = st;
                return frame;
            });

            Content = carouselView;
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
                        if (parts.Length == 3)
                        {
                            products.Add(new Product { Name = parts[0], Perevod = parts[1], Image = parts[2] });
                        }
                    }
                    return products;
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
                Console.WriteLine($"Ошибка при сохранении словаря: {ex.Message}");
            }
        }

        // Метод для добавления нового слова в словарь
        private void AddWordToDictionary(string name, string perevod, string image)
        {
            List<Product> products = LoadDictionaryFromFile() ?? new List<Product>();
            products.Add(new Product { Name = name, Perevod = perevod, Image = image });
            SaveDictionaryToFile(products);
        }



    }


}
