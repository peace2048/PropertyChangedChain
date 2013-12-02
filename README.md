PropertyChangedChain
====================

PropertyChanged イベントを受けて、別オブジェクトのプロパティを自動更新する

次のような事をしたい。

```VB.net
Class XXX
  Implements INotifyPropertyChanged
  
  Private WithEvents _obj As yyy
  
  Public Sub New()
    _obj = New yyy()
  End Sub
  
  Public Property Name As String
  
  Private Sub obj_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) handled yyy.PropertyChanged
    If e.PropertyName = "Name" Then
      Me.Name = _obj.Name
      RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
    End If
  End Sub
  
  Public Event PropertyChanged As PropertyChangedEventHandler
End Class
```

を

```VB.net
Class XXX
  Implements INotifyPropertyChanged
  
  Private_obj As yyy
  
  Public Sub New()
    _obj = New yyy()
    Dim map = New PropertyChangedChain(_obj, Me)
    map.From(Function(o) o.Name).AssignAndRaise()
  End Sub
  
  Public Property Name As String

  Public Event PropertyChanged As PropertyChangedEventHandler
End Class
```

のように書きたい。

上記のように単純に同じ名前のプロパティをコピーするなら、
`map.From(Function(o) o.Name).AssignAndRaise()`
名前が違えば、
`map.From(Function(o) o.Name).To(Function(o) o.UserName).AssignAndRaise()`
と代入先のプロパティを指定する。
単純にコピーするだけ(PropertyChangedを発生させない)なら
`map.From(Function(o) o.Name).AssignOnly()`
とする。
コピー元の値を変えて代入するなら、
`map.From(Function(o) o.Name).Select(Function(value) value + "様").AssignAndRaise()`
とする。
ちょっと複雑に
`map.From(Function(o) o.Count).Select(Function(value) String.Format("{0}回", value))
.To(Function(o) o.ErrorCount).AssignAndRaise()`
みたいに書けると良いな。

で、次のような物を書いてみました。

```csharp

var source = new SampleClass1();
var target = new SampleClass2();
var map = source.AsPropertyChangedChain();
map.From(a => a.Id).AssignAndRaisePropertyChanged(targer, a => a.Id);
map.From(a => a.Age).Select(age => age >= 20).AssignTo(target, a => a.IsAdult);

```

諸事情で C# で書いています。
INotifyPropertyChanged を実装しているオブジェクトから、PropertyChangedChain<T> のオブジェクト
を作成し、From で、プロパティを IObservable<TValue> に変換する形で監視できるようにしています。
後は、Reactive Extensions で Linq と同じように Where でフィルタするなり、Select で
変換するなりしてください。
IObservable<TValue> を AssignTo でターゲットのプロパティに代入するオブザーバを作成し購読します。
また、AssignAndRaisePropertyChanged では代入と同時に PropertyChanged イベントを発行します。