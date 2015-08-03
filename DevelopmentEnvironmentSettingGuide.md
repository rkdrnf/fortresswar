# 개발툴 설치 #

**Unity 4.6**
http://unity3d.com/unity/download

**Visual Studio 2013**
https://www.dreamspark.com/Product/Product.aspx?productid=93

**Visual Studio 2013 Tools for Unity**
https://visualstudiogallery.msdn.microsoft.com/20b80b8c-659b-45ef-96c1-437828fe7cf2

-> VS에서 유니티 디버깅 가능하게 해주는 툴, VS 먼저 설치 필요


# Configuration #

1. /File/ - /Build Settings.../ - Platform에서 PC, Mac & Linux Standalone 선택 - 오른쪽에서 Development Build, Script Debugging 체크

2. /Assets/ - /Import Package/ - /Visual Studio 2012 Tools/ 선택 후 불러오기

3. /Edit/ - /Preferences.../ - /External Tools/에서
External Script Editor : UnityVS.OpenFile
External Script Editor Args : "($File)" $(Line)
Editor Attaching 체크 되어있는지 확인

4. /Visual Studio/ - /Open In Visual Studio/로 실행
(앞으로 이걸로 VS 열면 됨)


**프로젝트 자동 열기 해제**

/Edit/ - /Preferences.../ - /General/에서 Always Show Project Wizard 체크