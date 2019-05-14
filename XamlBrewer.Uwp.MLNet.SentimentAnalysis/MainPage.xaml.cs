using Microsoft.ML;
using System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlBrewer.Uwp.MLNet.SentimentAnalysis
{
    public sealed partial class MainPage : Page
    {
        private PredictionEngine<SentimentData, SentimentPrediction> _engine;

        public MainPage()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar != null)
            {
                titleBar.BackgroundColor = Colors.Transparent;
                titleBar.ForegroundColor = Colors.LightSlateGray;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.LightSlateGray;
                titleBar.ButtonForegroundColor = Colors.LightSlateGray;
            }

            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var mlContext = new MLContext(seed: null);

            var appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assets = await appInstalledFolder.GetFolderAsync("Assets");

            // Original zip file at
            // https://github.com/dotnet/samples/tree/master/machine-learning/models/sentimentanalysis
            var file = await assets.GetFileAsync("sentiment_model.zip");
            var filePath = file.Path;

            var model = mlContext.Model.Load(
                filePath: filePath,
                inputSchema: out _);

            _engine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
        }

        public class SentimentData
        {
            public string SentimentText;
        }

        public class SentimentPrediction
        {
            public bool PredictedLabel { get; set; }

            public float Probability { get; set; }

            public float Score { get; set; }

            public string SentimentAsText => PredictedLabel ? "positive" : "negative";
        }

        //// For reference, these were the input and output classes for training:
        ////
        ////public class SentimentData
        ////{
        ////    public string SentimentText;
        ////    [ColumnName("Label")]
        ////    public bool Sentiment;
        ////}
        ////
        ////public class SentimentPrediction : SentimentData
        ////{
        ////    [ColumnName("PredictedLabel")]
        ////    public bool Prediction { get; set; }
        ////    public float Probability { get; set; }
        ////    public float Score { get; set; }
        ////}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = _engine.Predict(new SentimentData { SentimentText = RatingText.Text });
            ResultText.Text = $"With a score of {result.Score} we are {result.Probability * 100}% sure that the tone of your comment is {result.SentimentAsText}.";

            var stars = RatingStars.Value;
            if ((stars <= 2 && result.PredictedLabel) || (stars >= 4 && !result.PredictedLabel))
            {
                ResultText.Text += Environment.NewLine + Environment.NewLine + "This does NOT correspond to the number of stars you granted.";
                ResultText.Text += Environment.NewLine + Environment.NewLine + "We are not amused.";
                PassedImage.Visibility = Visibility.Collapsed;
                FailedImage.Visibility = Visibility.Visible;
            }
            else
            {
                ResultText.Text += Environment.NewLine + Environment.NewLine + "This corresponds to the number of stars you granted.";
                ResultText.Text += Environment.NewLine + Environment.NewLine + "We take your feedback seriously.";
                FailedImage.Visibility = Visibility.Collapsed;
                PassedImage.Visibility = Visibility.Visible;
            }
        }
    }
}
