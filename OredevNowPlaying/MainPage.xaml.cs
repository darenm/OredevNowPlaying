using Microsoft.Graphics.Canvas.Effects;
using Robmikh.CompositionSurfaceFactory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using AnimationDirection = Windows.UI.Composition.AnimationDirection;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OredevNowPlaying
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();

            // Subscribe to Loaded and SizeChanged
            Loaded += OnLoaded;
            SizeChanged +=OnSizeChanged;

            // Use CoreApplication to extend into the title bar

            // Remove the solid-colored backgrounds behind the caption controls and system back button
            // Use ApplicationView
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            // Compositor

            // Load Image

            // Apply Saturation Effect (Win2D)

            // Add background to visual tree

            // Add Lighting

            // Animate Lights - Create expression template

            // Animate Lights - create implicit animations

            // Animate Lights - create timer

            // AnimateLights - initial move

            // Add Text (RobMikh.SurfaceFactory) Ø
        }

        private void LightTimerOnTick(object sender, object o)
        {
            MoveLights();
        }

        private void MoveLights()
        {
        }

        //private Vector3KeyFrameAnimation CreateTextAnimation()
        //{
        //}
    }
}
