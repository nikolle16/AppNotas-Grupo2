using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App_Notas___Grupo_2.Models;
using App_Notas___Grupo_2.Services;

namespace App_Notas___Grupo_2.Views;

public partial class Principal : ContentPage
{
	public Principal()
	{
		InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadNotas();
    }

    private async Task LoadNotas()
    {
        var userId = Preferences.Get("userId", null);
        if (userId != null)
        {
            var notas = await NotaService.GetNotasAsync(userId);
            NotasListView.ItemsSource = notas;
        }
        else
        {
            await DisplayAlert("Error", "No se pudo obtener el ID del usuario", "OK");
        }
    }

    private async void OnAgregarNotaClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Agregar Nota", "Cancel", null, "Nota de Texto", "Nota de Audio");
        switch (action)
        {
            case "Nota de Texto":
                await Navigation.PushAsync(new AggNota());
                break;
            case "Nota de Audio":
                await Navigation.PushAsync(new AggNotaAudio());
                break;
        }
    }
}