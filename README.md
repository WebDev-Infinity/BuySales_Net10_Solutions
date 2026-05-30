# 매입/매출 금액 관리

간단한 Windows Forms 기반 매입/매출 관리 프로그램입니다.

## 기술 구성

- .NET 10
- C# 10
- Windows Forms
- EF Core
- SQLite

## 주요 기능

- 로그인 없이 사용
- 날짜별 매입/매출 입력
- 품목, 단가, 수량, 금액 관리
- 단가와 수량 입력 시 금액 자동 계산
- 일별, 주별, 월별 매입/매출 합계 표시
- 큰 글자와 큰 버튼 중심의 화면
- 기본 다크 테마
- 라이트 테마 전환 및 설정 저장

## 데이터 보존

SQLite 데이터베이스는 실행 파일 폴더가 아닌 사용자 데이터 폴더에 저장됩니다.

```text
%LocalAppData%\BuySalesNet10\buysales.db
```

프로그램을 업데이트하더라도 이 데이터베이스 파일을 삭제하지 않으면 기존 데이터는 유지됩니다.

## 실행

```powershell
dotnet run --project .\src\BuySales.WinForms\BuySales.WinForms.csproj
```

## 빌드

```powershell
dotnet build .\BuySales_Net10_Solutions.slnx
```
