   M S B A M L   `   `   `  ����B  >HltvRss, Version=1.1.7.0, Culture=neutral, PublicKeyToken=null!clr-namespace:HltvRssHltvRss  5   	       HltvRss.GcmWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:HltvRss     5   	     � Title$  Send GCM message��5   	   $	��250��6"   $	��320��6/    � WindowStyle$ 
ToolWindow=�6;   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $��* �6   �� 5      $	��105 �6    � RowDefinitions 5   
   �� 5      $��30 �6   �� 5      $��* �6   �� 5      $��30 �6   .+�5   
   � $��0��6   $��5,0,0,0q�6   $��2��6-    5      �� 5      $��42 �6#   �� 5      $��* �6#   .+�5      �� $��Title:��6   $
��Left=�6%   $��1��6A   $��Center=�6N   $��0��6i   �� 5      -   5        $��tbTitle��5   &   $��1��6   $��23��65   $��0,0,10,0q�6A   5      �� 5   
   -   5         ��Name$ sendBtn��5      $��1��6"   $��2��62   $
��Send��6?   $��22��6N   $��85��6Z   6e   �� 5    
   -   5         ��Name$ tbMsg��5       $��10q�6!    �� TextWrapping$
 Wrap=�6-   $��2��6B   $��1��6V    �� AcceptsReturn . 6c   � 5!   
   $��2��6   $��0,0,0,0q�6    5"      �� 5#      $��42 �6#   �� 5$      $��* �6#   .+�5&      �� -   5         ��Name$ label��5&      $��Topic:��6#   $
��Left=�64   $��1��6P   $��Center=�6]   $��0��6x   �� 5'      -   5        	 ��Name$	 comboBox��5'      $��Center=�6)   $��1��6D   
 �� SelectedIndex$
 0��6T   .@�5(      �� -   5        $
��cbi1��5(      .��6,   global64   �� 5)      -   5        $
��cbi2��5)      .��6+   added62   5*      5,      50      51      