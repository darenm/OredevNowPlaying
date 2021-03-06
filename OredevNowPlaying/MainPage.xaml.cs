﻿using Microsoft.Graphics.Canvas.Effects;
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
        private Compositor _compositor;
        private LoadedImageSurface _imageSurface;
        private SpriteVisual _backgroundVisual;
        private ContainerVisual _containerVisual;
        private PointLight _pointLight1;
        private PointLight _pointLight2;
        private AmbientLight _ambientLight;
        private ImplicitAnimationCollection _implicitOffsetAnimation;
        private DispatcherTimer _lightTimer;
        private bool _lightFlip;
        private SurfaceFactory _surfaceFactory;
        private TextSurface _textSurface;
        private SpriteVisual _textVisual;

        public MainPage()
        {
            this.InitializeComponent();

            // Subscribe to Loaded and SizeChanged
            Loaded += OnLoaded;
            SizeChanged +=OnSizeChanged;

            // Use CoreApplication to extend into the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            // Remove the solid-colored backgrounds behind the caption controls and system back button
            // Use ApplicationView
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (_backgroundVisual == null)
            {
                return;
            }

            _backgroundVisual.Size = RootElement.RenderSize.ToVector2();
            _textVisual.Offset = new Vector3((float)RootElement.ActualWidth / 2f, (float)RootElement.ActualHeight / 2f, 0);
            _textVisual.StartAnimation(nameof(Visual.Offset), CreateTextAnimation());
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            // Compositor
            _compositor = Window.Current.Compositor;

            // Load Image
            _imageSurface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/webster-pass.png"));
            var backgroundBrush = _compositor.CreateSurfaceBrush(_imageSurface);
            backgroundBrush.Stretch = CompositionStretch.UniformToFill;

            // Apply Saturation Effect (Win2D)
            var saturationEffect = new SaturationEffect
            {
                Saturation = 0,
                Source = new CompositionEffectSourceParameter("background")
            };

            var saturationEffectFactory = _compositor.CreateEffectFactory(saturationEffect);
            var saturationBrush = saturationEffectFactory.CreateBrush();
            saturationBrush.SetSourceParameter("background", backgroundBrush);

            // Add background to visual tree
            _backgroundVisual = _compositor.CreateSpriteVisual();
            _backgroundVisual.Brush = saturationBrush;
            _backgroundVisual.Size = RootElement.RenderSize.ToVector2();

            _containerVisual = _compositor.CreateContainerVisual();
            _containerVisual.Children.InsertAtBottom(_backgroundVisual);
            ElementCompositionPreview.SetElementChildVisual(RootElement, _containerVisual);

            // Add Lighting
            _ambientLight = _compositor.CreateAmbientLight();
            _ambientLight.Intensity = 1.5f;
            _ambientLight.Color = Colors.Purple;
            _ambientLight.Targets.Add(_backgroundVisual);
            
            _pointLight1 = _compositor.CreatePointLight();
            _pointLight1.Color = Colors.Yellow;
            _pointLight1.CoordinateSpace = _containerVisual;
            _pointLight1.Targets.Add(_backgroundVisual);
            _pointLight1.Offset = new Vector3((float)RootElement.ActualWidth, (float)RootElement.ActualHeight * 0.25f, 300);

            _pointLight2 = _compositor.CreatePointLight();
            _pointLight2.Color = Colors.Green;
            _pointLight2.Intensity = 2f;
            _pointLight2.CoordinateSpace = _containerVisual;
            _pointLight2.Targets.Add(_backgroundVisual);
            _pointLight2.Offset = new Vector3(0, (float)RootElement.ActualHeight * 0.75f, 300);

            // Animate Lights - Create expression template
            var offsetAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = nameof(PointLight.Offset);
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromSeconds(10);

            // Animate Lights - create implicit animations
            _implicitOffsetAnimation = _compositor.CreateImplicitAnimationCollection();
            _implicitOffsetAnimation[nameof(Visual.Offset)] = offsetAnimation;

            _pointLight1.ImplicitAnimations = _implicitOffsetAnimation;
            _pointLight2.ImplicitAnimations = _implicitOffsetAnimation;

            // Animate Lights - create timer
            _lightTimer = new DispatcherTimer();
            _lightTimer.Interval = TimeSpan.FromSeconds(10);
            _lightTimer.Tick += LightTimerOnTick;
            _lightTimer.Start();

            // AnimateLights - initial move
            MoveLights();

            // Add Text (RobMikh.SurfaceFactory) Ø
            _surfaceFactory = SurfaceFactory.GetSharedSurfaceFactoryForCompositor(_compositor);
            _textSurface = _surfaceFactory.CreateTextSurface("Hello Øredev!");
            _textSurface.ForegroundColor = Color.FromArgb(50, 255, 255, 255);
            _textSurface.FontSize = 150;
            var textBrush = _compositor.CreateSurfaceBrush(_textSurface.Surface);
            _textVisual = _compositor.CreateSpriteVisual();
            _textVisual.Size = _textSurface.Size.ToVector2();
            _textVisual.RotationAngleInDegrees = 45f;
            _textVisual.AnchorPoint = new Vector2(0.5f);
            _textVisual.Brush = textBrush;
            _textVisual.StartAnimation(nameof(Visual.Offset), CreateTextAnimation());

            _containerVisual.Children.InsertAtTop(_textVisual);
        }

        private void LightTimerOnTick(object sender, object o)
        {
            MoveLights();
        }

        private void MoveLights()
        {
            if (_lightFlip)
            {
                _pointLight1.Offset = new Vector3((float)RootElement.ActualWidth, (float)RootElement.ActualHeight * 0.25f,
                    300);
                _pointLight2.Offset = new Vector3(0, (float)RootElement.ActualHeight * 0.75f, 300);
            }
            else
            {
                _pointLight2.Offset = new Vector3((float)RootElement.ActualWidth, (float)RootElement.ActualHeight * 0.25f,
                    300);
                _pointLight1.Offset = new Vector3(0, (float)RootElement.ActualHeight * 0.75f, 300);
            }
            _lightFlip = !_lightFlip;
        }

        private Vector3KeyFrameAnimation CreateTextAnimation()
        {
            var centerOffset = new Vector3((float) RootElement.ActualWidth / 2, (float) RootElement.ActualWidth / 2, 0);
            var offsetAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Duration = TimeSpan.FromSeconds(10);
            offsetAnimation.InsertKeyFrame(0f, new Vector3(-2000, -2000, 0));
            offsetAnimation.InsertKeyFrame(0.5f, centerOffset);
            offsetAnimation.InsertKeyFrame(1f, new Vector3(2000, 2000, 0));
            offsetAnimation.Direction = AnimationDirection.Alternate;
            offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            return offsetAnimation;
        }
    }
}
