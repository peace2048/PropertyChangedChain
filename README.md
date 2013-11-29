PropertyChangedChain
====================

PropertyChanged イベントを受けて、別オブジェクトのプロパティを自動更新する

次のような事をしたい。

```VB.net
Class XXX
  Implements INotifyPropertyChanged
  
  Private WithEvents _obj As xxx
  
  Public Sub New()
    _obj = New xxx()
  End Sub
  
  Public Property Name As String
  
  Private Sub xxx_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) handled xxx.PropertyChanged
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
  
  Private_obj As xxx
  
  Public Sub New()
    _obj = New xxx()
    Dim map = New PropertyChangedChain(_obj, Me)
    map.From(Function(o) o.Name).AssignAndRaise()
  End Sub
  
  Public Property Name As String

  Public Event PropertyChanged As PropertyChangedEventHandler
End Class
```
