using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiniaturaVideo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MediaPlayer midiaPlayer = new MediaPlayer();
        string _arquivoMidia;
        const int LARGURA_MINIATURA = 192;
        const int ALTURA_MINIATURA = 108; 

        public MainWindow()
        {
            InitializeComponent();

            // evento para exibir as informações sobre a mídia selecionada
            midiaPlayer.MediaOpened += MidiaPlayer_MediaOpened;

            // define a borda como área visual do player na tela
            VideoDrawing videoDrawing = new VideoDrawing();
            videoDrawing.Player = midiaPlayer;
            videoDrawing.Rect = new Rect(0, 0, MidiaPlayerBorder.Width, MidiaPlayerBorder.Height);
            DrawingBrush drawingBrush = new DrawingBrush(videoDrawing);
            MidiaPlayerBorder.Background = drawingBrush;
        }

        private void AbrirVideoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileOpenDialog = new OpenFileDialog();
            fileOpenDialog.Filter = "Vídeos (*.mp4;*.avi;*.mpg)|*.mp4;*.mpg;*.avi";
            if (fileOpenDialog.ShowDialog() == true)
            {
                _arquivoMidia = fileOpenDialog.FileName;
                midiaPlayer.Open(new Uri(fileOpenDialog.FileName, UriKind.Absolute));
                midiaPlayer.Play();
            }
        }

        private void MidiaPlayer_MediaOpened(object sender, EventArgs e)
        {
            InfosMidiaTextBlock.Text = $"Mídia: {_arquivoMidia}\nLargura: {midiaPlayer.NaturalVideoWidth}\nAltura: {midiaPlayer.NaturalVideoHeight}";
            ObterMiniaturaButton.IsEnabled = (midiaPlayer.NaturalVideoWidth > 0 && midiaPlayer.NaturalVideoHeight > 0);
            PausarVideoButton.Content = " Pausar ";
        }

        private void ObterMiniaturaButton_Click(object sender, RoutedEventArgs e)
        {

            // cria uma área de imagem e pinta nela o quadro atual da mídia no player
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawVideo(midiaPlayer, new Rect(0, 0, midiaPlayer.NaturalVideoWidth, midiaPlayer.NaturalVideoHeight));
            }
            RenderTargetBitmap imagemOriginal = new RenderTargetBitmap(midiaPlayer.NaturalVideoWidth, midiaPlayer.NaturalVideoHeight, 96, 96, PixelFormats.Default);
            imagemOriginal.Render(drawingVisual);

            // calcula a nova escala, mantendo o aspect ratio
            int novaLargura = Math.Min(imagemOriginal.PixelWidth * ALTURA_MINIATURA / imagemOriginal.PixelHeight, LARGURA_MINIATURA);
            int novaAltura = Math.Min(imagemOriginal.PixelHeight * LARGURA_MINIATURA / imagemOriginal.PixelWidth, ALTURA_MINIATURA);

            // cria a área com as novas medidas da imagem, centralizando-a
            Rect rect = new Rect((LARGURA_MINIATURA - novaLargura) / 2, (ALTURA_MINIATURA - novaAltura) / 2, novaLargura, novaAltura);

            // redesenha a imagem original, dentro do retângulo com as novas medidas da área
            DrawingGroup drawingGroup = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(drawingGroup, BitmapScalingMode.HighQuality);
            drawingGroup.Children.Add(new ImageDrawing(imagemOriginal, rect));

            // finaliza o desenho da imagem já com as novas dimensões
            drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(drawingGroup);
            }
            RenderTargetBitmap imagemFinal = new RenderTargetBitmap(LARGURA_MINIATURA, ALTURA_MINIATURA, 96, 96, PixelFormats.Default);
            imagemFinal.Render(drawingVisual);

            // exibe a imagem de miniatura
            MiniaturaImage.Source = BitmapFrame.Create(imagemFinal);

        }

        private void PausarVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PausarVideoButton.Content.ToString() == " Pausar ")
            {
                PausarVideoButton.Content = " Tocar ";
                midiaPlayer.Pause();
            }
            else
            {
                PausarVideoButton.Content = " Pausar ";
                midiaPlayer.Play();
            }
        }
    }
}
