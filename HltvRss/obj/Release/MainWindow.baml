   M S B A M L   `   `   `  ���� B  >HltvRss, Version=1.1.7.0, Culture=neutral, PublicKeyToken=null!clr-namespace:HltvRssHltvRss      HltvRss.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     'localclr-namespace:HltvRss     -     � Title$  HLTV Server!��$	��480��$	��420�� � WindowStyle$ 
ToolWindow=� � 	Resources  �� Color��%�   (��       * SystemColors.HighlightBrushKey+)(��      *SystemColors.ControlBrushKey+)�� $ Transparent���� $ Transparent��.���  � ColumnDefinitions �� $	��50* ��� $	��50* � � RowDefinitions �� $��* ��� $��85 ��� $��35 �.+��� -    ��Name$ lb_hot��$��5,20,5,0q�$��0��$��0����.  ���� .R���  �� CornerRadius$ 3��$��30��$	��150�����	#FF122B18$��1q�$��0,1,0,0q�����  �� EndPoint$ 0.5,1&�	 �� 
StartPoint$	 0.5,0&�.��� 
 � Color$
 	#59A8A8A8�� � Offset$ 0Q�� $
 	#5980817F��$ 1Q�._�� $
��Left=� �� $��* ��� $��* �.+���  ��Name$ lblName��$��0��$��3,0,0,0q�$��0q�$��9.333(����� *TeamOne.Text+�� $ lblTemp��$��1��$��3,0,0,0q�$��0q�$��9.333(����� *TeamTwo.Text+�� -   $ 	lb_result��$��5,20,5,0q�$��1��$��0��$��Disabled=����� .R��� $ 3��$��30��$	��150�����	#FF122B18$��1q�$��0,1,0,0q����� $ 0.5,1&�$	 0.5,0&�.��� $
 	#59A8A8A8��$ 0Q�� $
 	#5980817F��$ 1Q�._�� $
��Left=� �� $��* ��� $��* �.+��� $ lblName��$��0��$��3,0,0,0q�$��0q�$��9.333(����� *TeamOne.Text+�� $ lblTemp��$��1��$��3,0,0,0q�$��0q�$��9.333(����� *TeamTwo.Text+� $��0��$��0��$��2�� �� $	��50* ��� $	��50* �.+��� -   $ lbHot��$��Hot matches��$
��Left=�$��0,0,0,0q�$	��Top=�$��0��$��0���� -   $ label1��$��Latest result��$
��Left=�$��0,0,0,0q�$	��Top=�$��0��$��1���� -   $ lbAdd��$��
Loading...��$
��Left=�$��2,0,0,0q�$	��Top=�$��10.667(�$��1���� -   $ lbRemove��$��
Loading...��$
��Left=�$��2,15,0,0q�$	��Top=�$��10.667(�$��1��$��0���� -   $ lbClient��$��
Loading...��$
��Left=�$��2,30,0,0q�$	��Top=�$��10.667(�$��1��$��0���� -	   $ lbGcm��$��
Loading...��$
��Left=�$��2,45,0,0q�$	��Top=�$��10.667(�$��1��$��0���� -
   $ lbSize��$��
Loading...��$
��Left=�$��0,0,0,0q�$	��Top=�$��1��$��1���� -   $ lbrss��$��
Loading...��$
��Left=�$��0,15,0,0q�$	��Top=�$��1��$��1���� -   $ lbsock��$��
Loading...��$
��Left=�$��0,30,0,0q�$	��Top=�$��1��$��1���� -   $ lbTemp��$��
Loading...��$
��Left=�$��2,0,0,0q�$��Bottom=�$��10.667(�$��1��$��2��� $��2��$��0��$��2��.+��� $��Up time:��$��0,5,0,0q��� -   $ label2��$��Label��$
��Left=�$��50,5,0,0q�$	��Top=��� -    ��Name$ button2��$	��GCM��$��Right=�$��	0,5,165,0q�$	��Top=�$��75���� -   $ button��$��Hot matches��$��Right=�$��0,5,85,0q�$	��Top=�$��75���� -   $ button1��$��Results��$��Right=�$��0,5,5,0q�$	��Top=�$��75��