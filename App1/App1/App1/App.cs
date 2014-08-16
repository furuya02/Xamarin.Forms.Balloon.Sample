using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace App1{
    public class App{

        public static Page GetMainPage(){
            return new MyPage();
        }
    }

    //スピーカの位置
    internal enum Mouth{
        Left,
        Right
    }

    internal class Msg{
        public string Name { get; set; }
        public String Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    internal class MyPage : ContentPage{
        public MyPage(){
            var ar = new ObservableCollection<Msg>{
                new Msg{CreatedAt = new DateTime(2014, 08, 1, 10, 10, 0),Name = "Taro",Text = "始めまして！、これがバルーンビューなんですね！"},
                new Msg{
                    CreatedAt = new DateTime(2014, 08, 1, 10, 12, 0),
                    Name = "Me",
                    Text = "そうなんです。話す人の位置（右・左）とメッセージを指定するだけで、このようなバルーンが表示されるんです"
                },
                new Msg{CreatedAt = new DateTime(2014, 08, 1, 10, 14, 0), Name = "Hanako", Text = "いいですね"},
                new Msg{
                    CreatedAt = new DateTime(2014, 08, 1, 10, 15, 0),
                    Name = "Me",
                    Text = "長いメッセージは、横幅を超えると自動的に改行するのです"
                },
                new Msg{CreatedAt = new DateTime(2014, 08, 1, 10, 17, 0), Name = "Taro", Text = "なるほど"},
                new Msg{
                    CreatedAt = new DateTime(2014, 08, 1, 10, 18, 0),
                    Name = "Me",
                    Text = "１行でも、短いメッセージは、バルーンの横幅も短くなってるでしょ"
                },
                new Msg{CreatedAt = new DateTime(2014, 08, 1, 10, 20, 0), Name = "Taro", Text = "ほんとだ（＠＠）"},
                new Msg{CreatedAt = new DateTime(2014, 08, 1, 10, 20, 0), Name = "Hanako", Text = "すごい！💛"},
            };
            BackgroundImage = "back.png";
            Padding = new Thickness(0,Device.OnPlatform(20,0,0),0,0);
            var mainLayout = new StackLayout();
            foreach (var a in ar){
                mainLayout.Children.Add(CreateOneMsg(a.Name, a.Text, a.CreatedAt.ToString("hh:mm")));
            }
            Content = mainLayout;
        }

        private StackLayout CreateOneMsg(string name, string text, string createdAt){
            var mainLayout = new StackLayout();
            mainLayout.Orientation = StackOrientation.Horizontal;//横に並べる
            if (name != "Me") {
                mainLayout.Children.Add(new Image() {
                    Source = string.Format("{0}.png", name),
                    WidthRequest = 40,
                    HeightRequest = 40
                });
                var subLayout = new StackLayout();

                //ユーザ名
                subLayout.Children.Add(new Label {Text = name,Font=Font.SystemFontOfSize(15)});

                //バルーンメッセージ
                var baloon = new Balloon();
                baloon.Mouth = Mouth.Left;
                baloon.Text = text;
                subLayout.Children.Add(baloon);

                mainLayout.Children.Add(subLayout);
                //日付
                mainLayout.Children.Add(new Label{
                    Text = createdAt,
                    Font = Font.SystemFontOfSize(10),
                    VerticalOptions = LayoutOptions.End,
                });

            } else {
                //日付
                mainLayout.Children.Add(new Label{Text=createdAt,Font=Font.SystemFontOfSize(10),VerticalOptions = LayoutOptions.End});
                //バルーンメッセージ
                var baloon = new Balloon();
                baloon.Mouth = Mouth.Right;
                baloon.Text = text;
                mainLayout.Children.Add(baloon);

                mainLayout.HorizontalOptions = LayoutOptions.End;
            }
            return mainLayout;
        }
    }


    //バルーンビュー
    class Balloon : ContentView{
        //アブソレートレイアウトの生成
        private readonly AbsoluteLayout _absoluteLayout = new AbsoluteLayout();

        public Balloon(){

            //AbsoluteLayoutに各種のビューを配置する
            //InitLayout(mouth,text);

            //バルーンビューとしてアブソレートレイアウトのみを返す
            Content = _absoluteLayout;
        }

        protected override void OnPropertyChanged(string propertyName = null){
            base.OnPropertyChanged(propertyName);
            if (propertyName == "Text"){
                _absoluteLayout.Children.Clear();
                InitLayout();
            }
        }

        //バインディング可能なプロパティ
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof (string),
            typeof (Balloon), default(string), BindingMode.OneWay);

        public String Text{
            get { return (String) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly BindableProperty MouthProperty = BindableProperty.Create("Mouth", typeof (Mouth),
            typeof (Balloon), default(Mouth), BindingMode.OneWay);

        public Mouth Mouth{
            get { return (Mouth) GetValue(MouthProperty); }
            set { SetValue(MouthProperty, value); }
        }

        //AbsoluteLayoutに各種のビューを配置する
        //private void InitLayout(Mouth mouth, String text) {
        private void InitLayout(){
            //******************************************************
            //変数の初期化
            //******************************************************
            const int r = 15; //扇形のサイズ（フォントサイズの基準ともなる）
            var fontSize = GetFontSize(r); //フォントサイズ
            var cols = GetCols(Text); //１行の文字数
            var rows = GetRows(Text, cols); //行数
            var h = GetH(r, rows); //ビューの高さ
            var w = GetW(cols, fontSize, r); //ビューの幅
            var lm = GetLm(Mouth, r); //左余白
            var rm = GetRm(Mouth, r); //右余白

            //******************************************************
            //扇型画像を生成してアブソレートレイアウトに配置する
            //******************************************************
            _absoluteLayout.Children.Add(CreateSector(r, 0, Mouth), new Point(lm, 0)); //左上
            _absoluteLayout.Children.Add(CreateSector(r, 90, Mouth), new Point(w - r - rm, 0)); //右上
            _absoluteLayout.Children.Add(CreateSector(r, 180, Mouth), new Point(w - r - rm, h - r)); //右下
            _absoluteLayout.Children.Add(CreateSector(r, 270, Mouth), new Point(lm, h - r)); //左下

            //******************************************************
            //BoxViewを生成してアブソレートレイアウトに配置する
            //******************************************************
            //縦長のBoxView
            var color = Mouth == Mouth.Left ? Color.White : Color.Lime;
            var boxView1 = new BoxView{Color = color};
            _absoluteLayout.Children.Add(boxView1, new Rectangle(lm + r - 1, 0, w - r*3 + 1, h)); //左右を１ドットずつ広げる
            //横長のBoxView
            var boxView2 = new BoxView{Color = color};
            _absoluteLayout.Children.Add(boxView2, new Rectangle(lm, r - 1, w - r, h - r*2 + 1)); //上下を１ドットずつ広げる

            //******************************************************
            //吹き出し口画像を生成してアブソレートレイアウトに配置する
            //******************************************************
            var point = Mouth == Mouth.Left ? new Point(r/4, r/2) : new Point(w - r - r/4, r/2);
            _absoluteLayout.Children.Add(CreateNozzle(r, Mouth), point);

            //******************************************************
            //メッセージを生成してアブソレートレイアウトに配置する
            //******************************************************
            var body = new Label();
            body.TextColor = Color.Black;
            body.Font = Font.SystemFontOfSize(fontSize);
            body.Text = Text;
            body.WidthRequest = cols*fontSize + Device.OnPlatform(0, r, 0);
            //１行の分の文字数を超えと改行するように幅を指定する(Androidだけちょっと幅が足りないので追加)
            _absoluteLayout.Children.Add(body, new Point(lm + r/2, r/2));
        }

        //フォントサイズ
        private int GetFontSize(int r){
            return r - 2; //基準の半径よりの小さくサイズとする
        }

        //１行の文字数
        private int GetCols(string text){
            var max = Device.OnPlatform(15,15,20);
            //もし、テキストが最大文字数以下の場合は、その文字数となる
            return (text.Length < max) ? text.Length : max;
        }

        //行数
        private int GetRows(string text, int cols){
            if (text.Length == 0){
                return 1; //0除算防止
            }
            //全文字数を１行の文字数で割ったもの、余りは１行となる
            return text.Length/cols + ((text.Length%cols) == 0 ? 0 : 1);
        }

        //ビューの高さ
        private int GetH(int r, int rows){
            //メッセージの行数と半径で決定される
            return r*rows + r;
        }

        //ビューの幅
        private int GetW(int cols, int fontSize, int r){
            //フォントサイズ×１行の文字数＋半径＋吹き出しサイズ
            return cols*fontSize + r + r;
        }

        //左余白
        private int GetLm(Mouth mouth, int r){
            //吹き出し分の余白(スピーカの位置によって変化する)
            return mouth == Mouth.Left ? r : 0;
        }

        //右余白
        private int GetRm(Mouth mouth, int r){
            //吹き出し分の余白(スピーカの位置によって変化する)
            return mouth == Mouth.Left ? 0 : r;
        }

        //扇形画像の生成　r:基準サイズ int:回転角度 Mouth:スピーカ位置
        private Image CreateSector(int r, int rotation, Mouth mouth){
            return new Image(){
                //画像ファイルは、スピーカ位置によって緑と白を使い分ける
                Source = mouth == Mouth.Left ? "sectorWhite.png" : "sectorLime.png",
                Rotation = rotation, //配置に応じて画像を回転させる
                WidthRequest = r, //基準サイズで初期化される
                HeightRequest = r
            };
        }

        //吹き出し画像の生成　r:基準サイズ Mouth:スピーカ位置
        private Image CreateNozzle(int r, Mouth mouth){
            return new Image{
                //画像ファイルは、スピーカ位置によって左右を使い分ける
                Source = mouth == Mouth.Left ? "nozzleWhite.png" : "nozzleLime.png",
                WidthRequest = r, //基準サイズで初期化される
                HeightRequest = r
            };
        }
    }
}