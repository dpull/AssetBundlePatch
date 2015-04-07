# AssetBundlePatch #

`AssetBundlePatch` 可以对 Unity3d 的 AssetBundle进行差异更新。

可以根据客户端安装包资源和已有的AssetBundle包，判断出重复资源，只进行差异更新。

该插件诞生过程：[http://www.dpull.com/blog/assetbundle/](http://www.dpull.com/blog/assetbundle/)

# 测试工程使用方法 #
测试环境：编辑器：unity4.6 mac, windows；移动设备：itouch4， 小米2

## iPhone ##
1. 打开XStudio->Tools->One Builder插件界面
1. 点击 Test iPhone 按钮
1. 运行 根目录中的 Bin/iPhone 文件夹中的XCode工程
1. 发布到手机上。

## Android ##
1. 打开XStudio->Tools->One Builder插件界面
1. 点击 Test Android 按钮
1. 使用Eclipse Import 根目录中的 Bin/Android 文件夹
1. 发布到手机上。

# 做了什么 #
该工程共有两个场景，一个小人跑在蓝色的场景上（Level1），一个小人跑在绿色的场景上（Level2），也就是说，除了场景的材质，其他的资源都是一样的。

我们的安装包通过BuildPlayer只Build了Level1，然后通过BuildStreamedSceneAssetBundle制作了Level2的AssetBundle，这个AssetBundle有2.93MB，但其实这个AssetBundle中的大部分资源都在客户端安装包中，使用 `AssetBundlePatch`将其取差异，将差异压缩后大小为19.4KB，然后客户端开始运行时，通过`AssetBundlePatch` 将其恢复为原来2.93MB的AssetBundle，合并完成后客户端将Enter Level2的按钮，可以进入绿色背景的Level2.


# 依赖库 #

* [`SharpZip.dll`](https://github.com/icsharpcode/SharpZipLib) 多种压缩格式的解压和压缩库。	
* [`AssetBundleParser.dll`](https://bitbucket.org/beings/assetbundleparser) AssetBundle文件的解析库，因为效率，写在了C中，其实就是 [`disunity`](https://github.com/ata4/disunity) 的C版本，发邮件到beingstudio@gmail.com获取代码读写权限。（邮件写上你的bitbucket账号）
