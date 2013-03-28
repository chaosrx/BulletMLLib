﻿using System;

namespace BulletMLLib
{
	/// <summary>
	/// BulletML Fire処理
	/// </summary>
	internal class BulletMLFire : BulletMLTask
	{
		#region Members

		BulletMLTree refNode, dirNode, spdNode, node, bulletNode;

		#endregion //Members

		#region Methods

		public BulletMLFire(BulletMLTree node)
		{
			this.node = node;
			this.dirNode = node.GetChild(BLName.direction);
			this.spdNode = node.GetChild(BLName.speed);
			this.refNode = node.GetChild(BLName.bulletRef);
			this.bulletNode = node.GetChild(BLName.bullet);

			if (dirNode == null && refNode != null)
			{
				dirNode = refNode.GetChild(BLName.direction);
			}

			if (dirNode == null && bulletNode != null)
			{
				dirNode = bulletNode.GetChild(BLName.direction);
			}

			if (spdNode == null && refNode != null)
			{
				spdNode = refNode.GetChild(BLName.speed);
			}

			if (spdNode == null && bulletNode != null)
			{
				spdNode = bulletNode.GetChild(BLName.speed);
			}

		}

		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			float changeDir = 0;
			float changeSpd = 0;

			// 方向の設定
			if (dirNode != null)
			{
				changeDir = (int)dirNode.GetValue(this) * (float)Math.PI / (float)180;
				if (dirNode.type == BLType.Sequence)
				{
					bullet.GetFireData().srcDir += changeDir;
				}
				else if (dirNode.type == BLType.Absolute)
				{
					bullet.GetFireData().srcDir = changeDir;
				}
				else if (dirNode.type == BLType.Relative)
				{
					bullet.GetFireData().srcDir = changeDir + bullet.Direction;
				}
				else
				{
					bullet.GetFireData().srcDir = changeDir + bullet.GetAimDir();
				}
			}
			else
			{
				bullet.GetFireData().srcDir = bullet.GetAimDir();
			}

			// 弾の生成
			BulletMLBullet newBullet = bullet.GetNewBullet();//bullet.tree);

			if (newBullet == null)
			{
				end = true;
				return BLRunStatus.End;
			}

			if (refNode != null)
			{
				// パラメータを取得
				for (int i = 0; i < refNode.children.Count; i++)
				{
					newBullet.tasks[0].paramList.Add(refNode.children[i].GetValue(this));
				}

				newBullet.Init(bullet.tree.GetLabelNode(refNode.label, BLName.bullet));
			}
			else
			{
				newBullet.Init(bulletNode);
			}

			newBullet.X = bullet.X;
			newBullet.Y = bullet.Y;
			newBullet.tasks[0].owner = this;
			newBullet.Direction = bullet.GetFireData().srcDir;

			if (!bullet.GetFireData().speedInit && newBullet.GetFireData().speedInit)
			{
				// 自分の弾発射速度の初期化がまだのとき、子供の弾の速度を使って初期値とする
				bullet.GetFireData().srcSpeed = newBullet.Speed;
				bullet.GetFireData().speedInit = true;
			}
			else
			{
				// 自分の弾発射速度の初期化済みのとき
				// スピードの設定
				if (spdNode != null)
				{
					changeSpd = spdNode.GetValue(this);
					if (spdNode.type == BLType.Sequence || spdNode.type == BLType.Relative)
					{
						bullet.GetFireData().srcSpeed += changeSpd;
					}
					else
					{
						bullet.GetFireData().srcSpeed = changeSpd;
					}
				}
				else
				{
					// 特に弾に速度が設定されていないとき
					if (!newBullet.GetFireData().speedInit)
					{
						bullet.GetFireData().srcSpeed = 1;
					}
					else
					{
						bullet.GetFireData().srcSpeed = newBullet.Speed;
					}
				}
			}

			newBullet.GetFireData().speedInit = false;
			newBullet.Speed = bullet.GetFireData().srcSpeed;

			end = true;
			return BLRunStatus.End;
		}

		#endregion //Methods
	}
}