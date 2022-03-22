### 浅拷贝和深拷贝

https://www.cnblogs.com/zk-zhou/p/6760277.html

相关API:  [IConeable.Clone()](https://docs.microsoft.com/zh-cn/dotnet/api/system.icloneable.clone?view=net-5.0#System_ICloneable_Clone)		浅拷贝 [Object.MemberwiseClone ()](https://docs.microsoft.com/zh-cn/dotnet/api/system.object.memberwiseclone?view=net-5.0#code-try-1)	MSDN示例中使用浅拷贝实现深拷贝DeepCopy

其实，用咱们在windows操作系统中使用的快捷方式和源文件的关系来理解就简单了！相信大家从玩电脑开始可能都遇到过这样的尴尬局面，就是用u盘复制了电脑的文件，然后兴高采烈的去打印，然后一到打印店打开u盘中复制到的文件，“纳尼！怎么打不开呢！原来是自己复制了一个快捷方式，额&……”（哈哈……说到这，估计有的人就非常有共鸣了啦！是吧？）
  快捷方式：其实就相当于是引用源文件，快捷方式中并不存在源文件对象，只是存放了一个源文件的地址，这个地址指向源文件，当你双击的时候，windows会根据这个地址去你的电脑寻找这个源文件并打开。只复制一个快捷方式，这就相当于是浅复制啦

复制源文件：将文件的数据都复制过来，这就是所谓的深复制。
https://blog.csdn.net/t131452n/article/details/42344067 简单理解

较好的理解和测试 https://www.cnblogs.com/xugang/archive/2010/09/09/1822555.html

注意string

### 两种拷贝与 等号赋值

https://blog.csdn.net/mohaze/article/details/93886582

三者解释



个人理解: 

```c#
class MyClass
{
   	public int i=0;
    public MyClass(int i)
    {
        this.i=i;
    }
}


class DemoClass 
{
    public int i=1;
    public MyClass myClass=new MyClass(5);
    
    public object ShallowCopy()
    {
        return this.MemberwiseClone();
    }
}
class Test
{
    static void Main(string[] args)
    {
        DemoClass A=new DemoClass();
        DemoClass B=(DemoClass)A.ShallowCopy();
    	
        B.i=2;
    
        Console.WriteLine("A.i={0},B.i={1}",A.i,B.i);
        
        B.myClass.i=10;
        
        Console.WriteLine("A.myClass.i={0},B.myClass.i={1}",A.myClass.i,B.myClass.i);
        
        B.myClass = null;
        Console.WriteLine("{0},{1}",A.myClass == null,B.myClass==null);
     
        DemoClass C = new DemoClass();
        C.myClass = new MyClass(8);
        B.myClass = C.myClass;
        Console.WriteLine("A.myClass.i={0},B.myClass.i={1},C.myClass.i={2}", A.myClass.i, B.myClass.i,C.myClass.i);
    }    
}
```

浅拷贝 值类型复制一份值	例如DemoClass中的i:

结果:A.i=1,B.i=2; B复制了一份A中的变量i;

引用类型复制一份引用	例如DemoClass中的myClass:

结果:A.myClass.i=10,B.myClass.i=10

​		False,True

​		A.myClass.i=10,B.myClass.i=8,C.myClass.i=8

;B复制了一份A中的myClass的引用(地址)



[C#深入解析深拷贝和浅拷贝](https://blog.csdn.net/weixin_34055787/article/details/85131264?spm=1001.2101.3001.6650.1&utm_medium=distribute.pc_relevant.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-1.pc_relevant_paycolumn_v3&depth_1-utm_source=distribute.pc_relevant.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-1.pc_relevant_paycolumn_v3&utm_relevant_index=2)

