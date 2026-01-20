# BottomSheetのカスタマイズ
## HandleとContentViewの調整
* ハンドル部分 ... BottomSheetHandle
* sheet部分 ... BottomSheetContentView

```xml
    <ContentPage.Resources>
        <ResourceDictionary>
            <!-- BottomSheetのハンドルを上にずらす。 -->
            <Style x:Key="TinySheetHandle" TargetType="telerik:BottomSheetHandle">
                <Setter Property="HeightRequest" Value="4" />
                <Setter Property="TranslationY" Value="-6" />
            </Style>
            <Style x:Key="TightSheetContent" TargetType="telerik:BottomSheetContentView">
                <!-- BottomSheetのコンテンツの上部の余白を減らす。 -->
                <Setter Property="Padding" Value="0,-15,0,0" />

                <!-- BottomSheetを右寄せにする。 -->
                <Setter Property="HorizontalOptions" Value="End" />
            </Style>
        </ResourceDictionary>
    </ContentPage.Resources>
```

Tabletではデフォルトで90%程度の幅になるので、幅を100%に設定する。
```xml
        <controls:RadBottomSheet
            BottomSheetContentWidth="100%" />
```

縦横で幅を変えたい場合は、VisualStateManagerを使う。
```xml
        <controls:RadBottomSheet>
            <VisualStateManager.VisualStateGroups>
                <VisualStateGroup x:Name="OrientationStates">
                    <VisualState x:Name="PortraitState">
                        <VisualState.StateTriggers>
                            <OrientationStateTrigger Orientation="Portrait" />
                        </VisualState.StateTriggers>
                        <VisualState.Setters>
                            <Setter Property="BottomSheetContentWidth" Value="100%" />
                        </VisualState.Setters>
                    </VisualState>
                    <VisualState x:Name="Landscape">
                        <VisualState.StateTriggers>
                            <OrientationStateTrigger Orientation="Landscape" />
                        </VisualState.StateTriggers>
                        <VisualState.Setters>
                            <Setter Property="BottomSheetContentWidth" Value="50%" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateManager.VisualStateGroups>
```