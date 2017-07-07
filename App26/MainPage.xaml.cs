using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App26 {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private Compositor _compositor;
        private Visual _visual;
        private Vector3KeyFrameAnimation _offsetAnimation;
        private ScalarKeyFrameAnimation _rotationAnimation;

        public MainPage() {
            this.InitializeComponent();
            this.Loaded += this.MainPage_Loaded;
            this.Unloaded += this.MainPage_Unloaded;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e) {
            _visual.Dispose();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e) {
            // InitComposition in Loaded event so the Size of the Visual is set
            InitComposition(rectRight);
            //InitCompositionWithRotate(rectRight);
        }

        private void ButtonLeft_Click(object sender, RoutedEventArgs e) {
            Reposition(rectLeft);
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e) {
            Reposition(rectRight);
        }

        private void ButtonBoth_Click(object sender, RoutedEventArgs e) {
            Reposition(rectLeft);
            Reposition(rectRight);
        }

        private void Reposition(FrameworkElement element) {
            int row = Grid.GetRow(element);
            int col = Grid.GetColumn(element);
            if (col == 0) {
                col++;
            } else {
                if (row == 0) {
                    row++;
                } else {
                    row = 0;
                }
                col = 0;
            }
            Grid.SetRow(element, row);
            Grid.SetColumn(element, col);
        }

        private void InitComposition(FrameworkElement element) {
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview")) {

                _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

                var easing = _compositor.CreateCubicBezierEasingFunction(
                        new Vector2(0.3f, 0.3f),
                        new Vector2(0.0f, 1.0f)
                );
                //var easing = _compositor.CreateLinearEasingFunction();

                _visual = ElementCompositionPreview.GetElementVisual(element);

                // Create animation
                _offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
                _offsetAnimation.Target = nameof(Visual.Offset);
                _offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue", easing);
                _offsetAnimation.Duration = TimeSpan.FromMilliseconds(SelectedDuration);

                // Set trigger + assign animation
                var implicitAnimations = _compositor.CreateImplicitAnimationCollection();
                implicitAnimations[nameof(Visual.Offset)] = _offsetAnimation;
                _visual.ImplicitAnimations = implicitAnimations;
            }
        }


        private void comboBoxDuration_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_offsetAnimation != null) {
                _offsetAnimation.Duration = TimeSpan.FromMilliseconds(SelectedDuration);
            }
            if (_rotationAnimation != null) {
                _rotationAnimation.Duration = _offsetAnimation.Duration;
            }
        }

        public int SelectedDuration => Convert.ToInt32((comboBoxDuration.SelectedItem as ComboBoxItem).Content);

        private void InitCompositionWithRotate(FrameworkElement element) {
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview")) {
                _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

                var easing = _compositor.CreateCubicBezierEasingFunction(
                        new Vector2(0.3f, 0.3f),
                        new Vector2(0.0f, 1.0f)
                );

                _visual = ElementCompositionPreview.GetElementVisual(element);

                // Set CenterPoint to the center, also when resized
                SizeChangedEventHandler fp = (sender, e) =>
                    _visual.CenterPoint = new Vector3((float)element.ActualWidth / 2f,
                                                        (float)element.ActualHeight / 2f, 0.0f);
                element.SizeChanged += fp;
                fp.Invoke(null, null);

                // Create animation
                _offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
                _offsetAnimation.Target = nameof(Visual.Offset);
                _offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue", easing);
                _offsetAnimation.Duration = TimeSpan.FromMilliseconds(SelectedDuration);

                _rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
                _rotationAnimation.Target = nameof(Visual.RotationAngleInDegrees);
                _rotationAnimation.InsertKeyFrame(0.0f, 0.0f);
                _rotationAnimation.InsertKeyFrame(1.0f, 360.0f);
                _rotationAnimation.Duration = _offsetAnimation.Duration;

                var animationGroup = _compositor.CreateAnimationGroup();
                animationGroup.Add(_offsetAnimation);
                animationGroup.Add(_rotationAnimation);

                // Set trigger + assign animation
                var implicitAnimations = _compositor.CreateImplicitAnimationCollection();
                implicitAnimations[nameof(Visual.Offset)] = animationGroup;
                _visual.ImplicitAnimations = implicitAnimations;
            }
        }

    }
}
