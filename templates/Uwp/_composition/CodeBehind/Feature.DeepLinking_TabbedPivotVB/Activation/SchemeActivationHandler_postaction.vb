﻿'{[{
Imports Param_ItemNamespace.Views
'}]}
Namespace Activation
    Friend Class SchemeActivationHandler
        Inherits ActivationHandler(Of ProtocolActivatedEventArgs)
        '{[{

        ' By default, this handler expects URIs of the format 'wtsapp:sample?paramName1=paramValue1&paramName2=paramValue2'
        Protected Overrides Async Function HandleInternalAsync(args As ProtocolActivatedEventArgs) As Task
            ' Create data from activation Uri in ProtocolActivatedEventArgs
            Dim data = New SchemeActivationData(args.Uri)
            If data.IsValid Then
                Dim frame = TryCast(Window.Current.Content, Frame)
                Dim pivotPage = TryCast(frame.Content, PivotPage)
                If pivotPage IsNot Nothing Then
                    Await pivotPage.InitializeFromSchemeActivationAsync(data)
                Else
                    NavigationService.Navigate(GetType(Views.PivotPage), data)
                End If
            ElseIf args.PreviousExecutionState <> ApplicationExecutionState.Running Then
                NavigationService.Navigate(GetType(Views.PivotPage))
            End If
            Await Task.CompletedTask
        End Function
        '}]}
    End Class
End Namespace
