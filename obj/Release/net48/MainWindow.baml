   M S B A M L   `   `   `  ����   ScannerBridge    ScannerBridge.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Document Scanner��$	��550��$	��460�� � 
ResizeMode$ NoResize=� � WindowStartupLocation$ CenterScreen=����#FAFAFA$��Segoe UI)�.��� $��20q� � RowDefinitions �� $
��Auto ��� $��* �.+��� ���#0063B1 �� Padding$ 14q� �� CornerRadius$ 8��$��0 0 0 20q�._��� $��🖨 Document Scanner��$��20(�$��SemiBold"�
�������$��Center=��� $��1�� �� Orientation$ Vertical=�$��Center=�$	��340�� � 	Resources  �� 
TargetType	 �� Property
 �� Value��%�   '��       �� " ��.��� 	 � ��$
 40���� 	 � ��$
 14(��� 	 � ��$

 10,4q��� 	 � ��

 ������� 	 � ��
 �#DDD�� 	 � ��$
 1q��� 	 � ��$
 0,0,0,6q�.+��� $��Select Scanner:��$��SemiBold"����#444�� -   $��ScannerComboBox���� $��Resolution (DPI):��$��SemiBold"����#444�� -   $��DpiComboBox��.@��� $	��100���� $	��150���� $	��200���� $	��300�� �� 
IsSelected$
 True���� $	��600���� $��Scan Source:��$��SemiBold"����#444�� -   $��SourceComboBox��.@��� $��Flatbed��$
 True���� $	��ADF���� $��Color Mode:��$��SemiBold"����#444�� -   $��ColorModeComboBox��.@��� $��Color��$
 True���� $��	Grayscale���� $��
BlackWhite���� -   $��
ScanButton��$��📄 Start Scan��$	��220��$��46��$��15(�$
��Bold"�
����������#28A745$��0q� � Cursor$
 Hand�� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     
BlurRadius$ 6Q�   ShadowDepth$ 1Q�   Opacity$
 0.25Q��� -    ��Name$ 
ScanStatus��$��Ready to scan...��$��13(����#444$��0,10,0,0q� �� TextAlignment$ Center=�$
��Wrap=�$��Center=�