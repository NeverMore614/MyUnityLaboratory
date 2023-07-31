## [真香预警！新版“峡谷第一美”妲己尾巴毛发制作分享](https://mp.weixin.qq.com/s/aIWMEO5Qa2gNn2yCmnHbOg "")这篇文章大佬讲得很详细了，其实就是多pass+采样一张alpha和nomal噪点图，还包含很详细的光照模型和偏移实现仿真的效果
### 这里讲一下我的多pass思路，URP不鼓励使用多pass（其实也是有办法的），并且考虑到多pass也不过是重复采样罢了，决定采用GPUInstance重复绘制mesh，使用instanceid作为pass层数来考虑，这种方式也可以实现效果。
