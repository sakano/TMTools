using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TMapCapture
{
    /// <summary>
    /// CaptureDragWindow.xaml の相互作用ロジック
    /// </summary>
    internal partial class CaptureDragWindow : Window
    {
        public CaptureDragWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 非同期にドラッグでの範囲選択を受付。選択されたらコールバックを呼び出す。
        /// </summary>
        /// <param name="handler">選択された範囲が渡されるハンドラ</param>
        public void ReceiveDragAsync(Action<Rect> handler)
        {
            Contract.Requires<ArgumentNullException>(handler != null);
            // ドラッグイベントを監視
            var resultRect = new Rect(); // ドラッグされた範囲
            var drag = draggedObservable();
            drag.Subscribe(rect =>
            {   // ドラッグ中はドラッグ範囲を記録しつつ表示
                resultRect = rect;
                setRectangleArea(rect);
            }, error =>
            {   // エラー時はウィンドウを閉じる
                Close();
            }, () =>
            {   // ドラッグ完了時はウィンドウを閉じてからハンドラを呼び出す
                Close();
                handler(resultRect);
            });

            fillWholeScreen();

            Show();
        }

        /// <summary>
        /// ウィンドウの位置とサイズを画面一杯にする
        /// </summary>
        private void fillWholeScreen()
        {
            Top = 0;
            Left = 0;
            Contract.Assume(SystemParameters.VirtualScreenWidth >= 0);
            Contract.Assume(SystemParameters.VirtualScreenHeight >= 0);
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
        }

        /// <summary>
        /// ドラッグ完了時にドラッグされた範囲を伝播するObservableを返す
        /// </summary>
        /// <returns>ドラッグされた時にドラッグ範囲を伝搬するObservable</returns>
        private IObservable<Rect> draggedObservable()
        {
            Contract.Ensures(Contract.Result<IObservable<Rect>>() != null);
            
            var mouseDown = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => h.Invoke, h => MouseLeftButtonDown += h, h => MouseLeftButtonDown -= h);
            var mouseMove = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(h => h.Invoke, h => MouseMove += h, h => MouseMove -= h);
            var mouseUp = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => h.Invoke, h => MouseLeftButtonUp += h, h => MouseLeftButtonUp -= h);
            var mouseDrag = mouseDown
                .Select(ev => ev.EventArgs.GetPosition(this))
                .CombineLatest(mouseMove,
                (downPos, ev) =>
                {
                    var movePos = ev.EventArgs.GetPosition(this);
                    return new Rect(Math.Min(downPos.X, movePos.X), Math.Min(downPos.Y, movePos.Y),
                        Math.Abs(downPos.X - movePos.X), Math.Abs(downPos.Y - movePos.Y));
                })
                .TakeUntil(mouseUp);
            
            Contract.Assume(mouseDrag != null);
            return mouseDrag;
        }

        /// <summary>
        /// 指定された領域を現すように矩形と線の位置を設定
        /// </summary>
        /// <param name="rect">表示する領域</param>
        private void setRectangleArea(Rect rect)
        {
            // 指定された範囲一杯に矩形
            Canvas.SetLeft(rectangle, rect.X);
            Canvas.SetTop(rectangle, rect.Y);
            rectangle.Width = rect.Width;
            rectangle.Height = rect.Height;

            // 指定された範囲の中心に縦横の線
            double left = rect.X, top = rect.Y;
            double width = rectangle.Width, height = rectangle.Height;
            horizontalLine.X1 = left;
            horizontalLine.Y1 = top + height / 2;
            horizontalLine.X2 = left + width;
            horizontalLine.Y2 = top + height / 2;

            verticalLine.X1 = left + width / 2;
            verticalLine.Y1 = top;
            verticalLine.X2 = left + width / 2;
            verticalLine.Y2 = top + height;
        }
    }
}
