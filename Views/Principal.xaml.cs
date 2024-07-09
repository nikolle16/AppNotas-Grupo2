namespace App_Notas___Grupo_2.Views;

public partial class Principal : ContentPage
{
	public Principal()
	{
		InitializeComponent();
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