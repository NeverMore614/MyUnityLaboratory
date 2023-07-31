## [真香预警！新版“峡谷第一美”妲己尾巴毛发制作分享](https://mp.weixin.qq.com/s/aIWMEO5Qa2gNn2yCmnHbOg "")这篇文章大佬讲得很详细了，其实就是多pass+采样一张alpha和nomal噪点图，还包含很详细的光照模型和偏移实现仿真的效果
### 这里讲一下我的多pass思路，URP不鼓励使用多pass（其实也是有办法的），并且考虑到多pass也不过是重复采样罢了，决定采用GPUInstance重复绘制mesh，使用instanceid作为pass层数来考虑，这种方式也可以实现效果。
不过这种方式会出现一个问题，需要管理各物体之间毛发的渲染顺序，因为毛发开启了透明混合（如果不开启会仅仅使用alphatest，模型就会像一个刺猬一样）来实现边缘那种模糊感，不控制渲染顺序的话就会出现穿透现象，就像下面这样
![不开启alphatest](/Img/furry1.PNG "")**不开启alphatest**

![开启alphatest](/Img/furry2.PNG "")**开启alphatest,但是透明交叉部分还是会有些许的问题**
