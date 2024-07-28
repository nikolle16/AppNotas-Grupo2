using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace App_Notas___Grupo_2.Config
{
    public class ImageContainer : StackLayout
    {
        public byte[] ImageData { get; private set; }

        public ImageContainer(byte[] imageData, Action<ImageContainer> onDelete)
        {
            ImageData = imageData;
            Orientation = StackOrientation.Vertical;

            var imageView = new Image
            {
                Source = ImageSource.FromStream(() => new MemoryStream(imageData)),
                HeightRequest = 100
            };

            var deleteButton = new Button
            {
                Text = "Eliminar",
                BackgroundColor = Colors.Red,
                TextColor = Colors.White
            };

            deleteButton.Clicked += (sender, e) => onDelete(this);

            Children.Add(imageView);
            Children.Add(deleteButton);
        }
    }
}

