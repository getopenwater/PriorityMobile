﻿Imports PrioritySFDC.TableView
Imports Priority

Public Class iFrmButtons
    Inherits iFormChild 'System.Windows.Forms.UserControl

#Region "Inheritance"

    Public Overrides ReadOnly Property ParentForm() As iForm
        Get
            Return _Parent.ParentForm
        End Get
    End Property

    Private _Parent As FormPanel
    Public Overloads ReadOnly Property Parent() As FormPanel
        Get
            Return _Parent
        End Get
    End Property

#End Region

#Region "Initialisation and finalisation"

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        For Each ob As Object In Me.Controls
            Dim btn As System.Windows.Forms.PictureBox = TryCast(ob, System.Windows.Forms.PictureBox)
            If Not IsNothing(btn) Then
                With btn
                    Dim ibtn As New iButton( _
                        .Tag, _
                        btn, _
                        btnEnabled.Images(.Tag), _
                        btnDisabled.Images(.Tag) _
                    )
                    AddHandler ibtn.ButtonClick, AddressOf hButtonClick
                    _Buttons.Add(.Tag, ibtn)
                End With
            End If
        Next

    End Sub

    Public Sub Load(ByRef Parent As FormPanel)
        _Parent = Parent
        ParentForm.thisHandler.DisabledButtons( _
            _Buttons(eFormButtons.btn_Add).Disabled, _
            _Buttons(eFormButtons.btn_Edit).Disabled, _
            _Buttons(eFormButtons.btn_Copy).Disabled, _
            _Buttons(eFormButtons.btn_Delete).Disabled, _
            _Buttons(eFormButtons.btn_Print).Disabled _
        )
    End Sub

#End Region

#Region "Buttons"

    Private _Buttons As New Dictionary(Of eFormButtons, iButton)
    Public Property Buttons() As Dictionary(Of eFormButtons, iButton)
        Get
            Return _Buttons
        End Get
        Set(ByVal value As Dictionary(Of eFormButtons, iButton))
            _Buttons = value
        End Set
    End Property

    Private Sub hButtonClick(ByVal Button As eFormButtons)

        Dim ex As New Exception
        With ParentForm.thisHandler
            Select Case Button
                Case eFormButtons.btn_Add
                    With _Parent
                        With .FormView.ViewForm
                            .Defocus(ex)
                        End With
                    End With
                    If IsNothing(ex) Then
                        With ParentForm.ViewMain.TableView
                            .ViewForm.Clear()
                            .EditItem = New cTableItem(.ViewTable.TableItem)
                        End With
                        .btn_AddPress(ParentForm)
                        With ParentForm.ViewMain.TableView
                            If .Container.Triggers.Keys.Contains("PRE-INSERT") Then
                                .Container.Triggers("PRE-INSERT").Execute()
                            End If
                        End With

                    Else
                        MsgBox(ex.Message, , _Parent.FormView.ViewForm.FocusedControl.thisColumn.Title)
                    End If

                Case eFormButtons.btn_Edit
                    With _Parent
                        With .FormView.ViewForm
                            .Defocus(ex)
                        End With
                    End With
                    If IsNothing(ex) Then
                        With ParentForm.ViewMain.TableView
                            .ViewForm.Clear()
                            .EditItem = .ViewTable.SelectedItem
                        End With
                        .btn_EditPress(ParentForm)
                    Else
                        MsgBox(ex.Message, , _Parent.FormView.ViewForm.FocusedControl.thisColumn.Title)
                    End If

                Case eFormButtons.btn_Copy
                        .btn_CopyPress(ParentForm)

                Case eFormButtons.btn_Delete
                        Select Case _Parent.TableView.TableView
                        Case eTableView.vTable
                            .btn_DeletePress(ParentForm)
                        Case eTableView.vForm
                            If MsgBox("Discard changes?", MsgBoxStyle.OkCancel, "Cancel Edit") = MsgBoxResult.Ok Then
                                With ParentForm.ViewMain.TableView
                                    .TableView = TableView.eTableView.vTable
                                    .ViewTable.Focus()
                                End With
                            End If
                    End Select


                Case eFormButtons.btn_Print
                        .btn_PrintPress(ParentForm)

                Case eFormButtons.btn_Post
                    Select Case _Parent.TableView.TableView
                        Case eTableView.vForm

                            _Parent.TableView.ViewForm.Defocus(ex)
                            If IsNothing(ex) Then

                                With _Parent.TableView
                                    Dim ei As cTableItem = .EditItem
                                    With .ViewTable
                                        .ClearSelection()
                                        With .thisTable
                                            If IsNothing(ei.EditItem) Then ' Adding
                                                .Items.Add(ei.thisItem)                                                
                                                .Items(.Items.Count - 1).Selected = True
                                                .Items(.Items.Count - 1).Focused = True
                                            Else ' Editing                                        
                                                .Items(ei.EditItem.Index) = ei.thisItem
                                                .Items(ei.EditItem.Index).Selected = True
                                                .Items(ei.EditItem.Index).Focused = True
                                            End If
                                            .Focus()
                                        End With
                                    End With
                                End With
                                .btn_SubmitPress(ParentForm)
                            Else
                                MsgBox(ex.Message)
                            End If

                        Case eTableView.vTable
                            Using xl As New Loading
                                Try
                                    xl.Clear()
                                    xl.Table = ParentForm.thisInterface.ldTable
                                    xl.Procedure = ParentForm.thisInterface.ldProcedure
                                    If Not IsNothing(ParentForm.thisInterface.ldEnv) Then
                                        xl.Environment = ParentForm.thisInterface.ldEnv
                                    End If

                                    .Post(ParentForm, xl)
                                    xl.Post(String.Format("{0}loadHandler.ashx", ParentForm.ue.Server), ex)

                                    If Not IsNothing(ex) Then
                                        MsgBox(ex.Message)
                                    Else
                                        .Close(ParentForm)
                                        ParentForm.Close()
                                    End If

                                Catch excep As Exception
                                    MsgBox(ex.Message)
                                End Try

                            End Using

                    End Select

            End Select
        End With

    End Sub

    Public Sub RefreshButtons()

        Try
            With ParentForm.ViewMain.TableView
                Select Case .TableView
                    Case eTableView.vForm
                        With Me
                            .Buttons(eFormButtons.btn_Add).Enabled = False
                            .Buttons(eFormButtons.btn_Edit).Enabled = False
                            .Buttons(eFormButtons.btn_Copy).Enabled = False
                            .Buttons(eFormButtons.btn_Delete).Enabled = True
                            .Buttons(eFormButtons.btn_Post).Enabled = ParentForm.ViewMain.TableView.ViewForm.HasMandatory
                        End With

                    Case eTableView.vTable
                        Dim hasSelected As Boolean = _
                            .Parent.TableView.ViewTable.HasSelection And _
                            .ViewTable.thisTable.Focused
                        With Me
                            .Buttons(eFormButtons.btn_Add).Enabled = TryCast(ParentForm.ViewMain.TableView.Container, cTable).CheckDependancy
                            .Buttons(eFormButtons.btn_Edit).Enabled = hasSelected
                            .Buttons(eFormButtons.btn_Copy).Enabled = hasSelected
                            .Buttons(eFormButtons.btn_Delete).Enabled = hasSelected
                            .Buttons(eFormButtons.btn_Post).Enabled = _
                                ParentForm.ViewMain.FormView.ViewForm.HasMandatory And _
                                ParentForm.ViewMain.TableView.ViewTable.HasMandatory And _
                                Not IsNothing(ParentForm.thisInterface.ldTable) And _
                                Not IsNothing(ParentForm.thisInterface.ldProcedure)
                        End With

                End Select

            End With

        Catch ex As Exception

        End Try

    End Sub

#End Region

End Class