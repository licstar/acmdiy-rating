acmdiy-rating
=============

ACM_DIY群Rating系统

## 简介

如果你是ACM_DIY群的成员，应该就知道这是什么……简单的说，就是根据每天群成员的表现，给每位成员算一个打分，活跃的成员分数会涨。同时这个系统还顺便提供了群聊记录搜索、热门统计等娱乐功能。

项目分成3部分，网页端、服务端、数据库。

其中网页端和服务端使用C# (ASP.Net)编写。数据库使用Sql Server。

期待有兴趣的朋友把它改写的更好一些。

代码很乱，读前请做好充分的心理准备，特别是服务端的代码。

## 使用配置

**1)** 配置数据库，用sql目录里的代码直接导入数据库即可。

**2)** 配置服务端

`server/Program.cs` 54行，数据库连接语句；936行，加一个登陆成功的Cookies

然后编译运行。程序就一直开着，每天00:06的时候，都会爬一次网站，更新一下前一天的聊天记录。

**3)** 配置网页端

`web/Web.config` 18行，数据库连接语句

## 待改进的

> 有建议可以直接写到这里，然后坐等有人来改。

### 群昵称获取

现有代码已经不能获取群成员的昵称了（那个API已经作废了，要换新的）

### 将检索功能替换为Lucene.Net

目前的搜索功能是信息检索课程的大作业，完全是自己写的，所有的索引都存在数据库里。虽然其中使用了trick实现了限制时间区间的检索功能，但实际上性能肯定不如Lucene等开源大项目。而且索引存在数据库里会很占空间。因此很有必要替换。

### 查看权限

目前群Rating的查看权限是所有人，其实只要知道地址的人，都能看。只用一个很水的验证码防住了无良的爬虫。如果可以和QQ的Openid绑定，后面很多事都会方便很多。

### 群Rating曲线

目前的曲线用的是Google的画图工具，访问URL长度有限，因此只能画最近40天的Rating，如果改用JQuery.plot之类的工具，无论从美观上还是数据表现力上，都会有提升。

## 版权

代码：GPLv3

使用了这些第三方库：

**SharpICTCLAS** （用来分词）

**Json.Net** （用来解析json）

**yaml** （网页界面）
