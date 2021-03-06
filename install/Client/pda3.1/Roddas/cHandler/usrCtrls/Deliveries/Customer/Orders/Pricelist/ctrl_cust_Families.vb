﻿Imports PriorityMobile
Imports System.Xml

Public Class ctrl_cust_Families
    Inherits iView

    'Private ReadOnly Property HomeForm() As xForm
    '    Get
    '        Return TopForm("Home").CurrentForm
    '    End Get
    'End Property

    'Private ReadOnly Property HomeFormView() As iView
    '    Get
    '        Return HomeForm.Views(HomeForm.CurrentView)
    '    End Get
    'End Property

#Region "Initialisation and Finalisation"

    Public Overrides ReadOnly Property MyControls() As ControlCollection
        Get
            Return Me.Controls
        End Get
    End Property

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        With Me
            With ListSort1
                .FormLabel = "Part Families"
                .Sort = "familytype"
                .AddColumn("familytype", "Family Type", 300, False)
                .AddColumn("familyname", "Family", 300, True)
            End With
        End With

    End Sub

#End Region

#Region "Overides Base Properties"

    'Public Overrides ReadOnly Property ButtomImage() As String
    '    Get
    '        Return "calendar.bmp"
    '    End Get
    'End Property

    Public Overrides ReadOnly Property Selected() As String
        Get
            Return ListSort1.Selected
        End Get
    End Property

    Public Overrides Sub FormClosing()
        thisForm.TopForm("Home").CurrentForm.Parent.Views(0).RefreshData()
        MyBase.FormClosing()
    End Sub

#End Region

#Region "Overrides base Methods"

    Public Overrides Sub Bind() Handles ListSort1.Bind

        IsBinding = True

        Dim dr() As Data.DataRow = Nothing

        Dim query As New System.Text.StringBuilder
        query.AppendFormat( _
            "{0} <> '0'", _
            ListSort1.Keys(0) _
        )

        Dim cust As XmlNode = thisForm.FormData.SelectSingleNode(thisForm.Parent.Parent.Parent.boundxPath)
        ListSort1.FormLabel = String.Format("Part Families for {0}.", cust.SelectSingleNode("custname").InnerText)
        query.Append(" AND ( 0 = 1")
        For Each f As XmlNode In cust.SelectNodes("custpart/family")
            query.AppendFormat( _
                " OR {0} = '{1}'", _
                "familyname", _
                f.Attributes("name").Value _
            )
        Next
        query.Append(")")

        dr = thisForm.Datasource.Select(query.ToString, ListSort1.Sort)

        With ListSort1
            .Clear()
            For Each r As System.Data.DataRow In dr
                .AddRow(r)
                If .RowSelected(r, thisForm.CurrentRow) Then
                    .Items(.Items.Count - 1).Selected = True
                    .Items(.Items.Count - 1).Focused = True
                Else
                    .Items(.Items.Count - 1).Selected = False
                    .Items(.Items.Count - 1).Focused = False
                End If
            Next
            .Focus()
        End With

        thisForm.RefreshSubForms()
        IsBinding = False

    End Sub

    Public Overrides Sub SetFocus()
        Me.ListSort1.Focus()
    End Sub

    Public Overrides Sub ViewChanged()
        Bind()
    End Sub

    Public Overrides Function SubFormVisible(ByVal Name As String) As Boolean
        With Me
            If IsNothing(.Selected) Then Return False
            If IsNothing(.thisForm.TableData.Current) Then Return False
            If IsNothing(ListSort1.Selected) Then Return False
            Select Case Name.ToUpper
                Case Else
                    Return True
            End Select
        End With
    End Function

#End Region

#Region "Local control Handlers"

    Private Sub hSelectedindexChanged(ByVal row As Integer) Handles ListSort1.SelectedIndexChanged
        If IsBinding Then Exit Sub
        With thisForm
            Dim cur As String = ListSort1.Value(ListSort1.Keys(0), row)
            If Not String.Compare(Text, cur) = 0 Then
                .TableData.Position = .TableData.Find(ListSort1.Keys(0), cur)
                .RefreshSubForms()
                .RefreshDirectActivations()
            End If
        End With
    End Sub

#End Region

#Region "Direct Activations"

    'Public Overrides Sub DirectActivations(ByRef ToolBar As daToolbar)
    '    ToolBar.Add(AddressOf hPlaceCall, "PHONE.BMP", thisForm.CurrentRow("phone").ToString.Length > 0)
    'End Sub

    'Private Sub hPlaceCall()

    '    Dim ph As New Microsoft.WindowsMobile.Telephony.Phone
    '    Try
    '        ph.Talk(thisForm.CurrentRow("phone"))
    '    Catch ex As Exception
    '        MsgBox(String.Format("Call failed to: {0}.", thisForm.CurrentRow("phone")))
    '    End Try

    'End Sub

#End Region

End Class
