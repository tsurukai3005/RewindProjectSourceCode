# RewindProjectSourceCode

### CameraManager.cs, CameraManagerEndlessStage.cs
カメラをプレイヤーに追従させる

### ObjectsMove.cs
オブジェクトの回転運動・往復運動を実現する

### ObjectsSpawner.cs
任意のオブジェクトを指定した角度・角速度・力で射出する

### PlayerAction.cs
プレイヤーの移動、ジャンプ、地面との接触判定を管理する

### PointInTime.cs
各フレームごとのオブジェクトの位置、角度、速度、加速を記録するデータ構造

### SpawnerData.cs
スポナーから射出されたオブジェクトの情報を記録するデータ構造

### SpawnerGenerate.cs
エンドレスステージにて、スポナーをランダムな位置に生成する

### TimeBody.cs
時間の停止・再生・逆再生でオブジェクトの物理的性質を切り替える
オブジェクトの過去の軌跡を利用して残像を生成する

### TimeManager.cs
時間の停止・再生・逆再生の入力を判定する
経過した時間と入力から時間の状態を決定する

### UIActivatorOnCollision.cs
プレイヤーが特定の領域に入るとUIで説明文を出現させる（チュートリアル用）
