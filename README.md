# 매입/매출 금액 관리

## 프로그램 개요

어르신도 쉽게 사용할 수 있도록 만든 간단한 매입/매출 장부 프로그램입니다.
복잡한 로그인이나 서버 연결 없이, 날짜별로 품목, 단가, 수량을 입력해 매입/매출 내역을 기록하는 데 목적이 있습니다.

입력한 데이터는 외부 서버로 전송되지 않고, 사용 중인 PC의 로컬 SQLite 데이터베이스에만 저장됩니다.

이 프로젝트는 Codex AI를 이용해 개발되었습니다.

## 기술 구성

- .NET 10
- C# 10
- Windows Forms
- EF Core
- SQLite

## 주요 기능

- 로그인 없이 바로 사용
- 프로그램 시작 시 창 최대화
- 매출을 기본 선택값으로 시작
- 저장 후에도 선택한 날짜와 매입/매출 구분을 유지하여 연속 입력 가능
- 품목, 단가, 수량 입력
- 단가와 수량을 기준으로 금액 자동 계산
- 단가는 1원 이상만 저장 가능
- 수량은 정수로 입력
- 금액은 직접 입력하지 않는 읽기 전용 표시값
- Enter 키로 품목, 단가, 수량, 메모 순서 이동
- 메모에서 Enter 입력 시 자동 저장
- 선택한 날짜 기준 일별 목록 조회
- 일별, 주별, 월별 매입/매출 합계 표시
- 일별/월별 엑셀 CSV 내보내기
- 다크 테마와 라이트 테마 전환
- 큰 글자와 큰 버튼 중심의 화면
- 단일 실행 파일 게시 지원

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

## 게시

프로젝트 파일에 단일 실행 파일 게시 옵션이 설정되어 있으므로 다음 명령으로 게시할 수 있습니다.

```powershell
dotnet publish .\src\BuySales.WinForms\BuySales.WinForms.csproj -c Release
```

게시 결과물은 다음 경로에 생성됩니다.

```text
src\BuySales.WinForms\bin\Release\net10.0-windows\win-x64\publish\BuySales.WinForms.exe
```
