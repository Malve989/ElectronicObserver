﻿using ElectronicObserver.Utility.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicObserver.Data {
	
	/// <summary>
	/// 艦隊の情報を保持します。
	/// </summary>
	[DebuggerDisplay( "[{ID}] : {Name}" )]
	public class FleetData : APIWrapper, IIdentifiable {

		/// <summary>
		/// 艦隊ID
		/// </summary>
		public int FleetID {
			get { return (int)RawData.api_id; }
		}

		/// <summary>
		/// 艦隊名
		/// </summary>
		public string Name {
			get { return (string)RawData.api_name; }
		}

		/// <summary>
		/// 遠征状態
		/// 0=未出撃, 1=遠征中, 2=遠征帰投, 3=強制帰投中
		/// </summary>
		public int ExpeditionState { get; internal set;	}

		/// <summary>
		/// 遠征先ID
		/// </summary>
		public int ExpeditionDestination { get; internal set; }

		/// <summary>
		/// 遠征帰投時間
		/// </summary>
		public DateTime ExpeditionTime { get; internal set; }


		private int[] _fleetMember;
		/// <summary>
		/// 艦隊メンバー
		/// </summary>
		public ReadOnlyCollection<int> FleetMember {
			get { return Array.AsReadOnly<int>( _fleetMember ); }
		}


		public int ID {
			get { return FleetID; }
		}



		public override void LoadFromResponse( string apiname, dynamic data ) {
		
			switch ( apiname ) {

				case "api_req_mission/start":
					ExpeditionTime = DateConverter.FromAPITime( (long)data.api_complatetime );
					break;

				default:			//checkme
					base.LoadFromResponse( apiname, (object)data );

					_fleetMember = (int[])RawData.api_ship;
					ExpeditionState = (int)RawData.api_mission[0];
					ExpeditionDestination = (int)RawData.api_mission[1];
					ExpeditionTime = DateConverter.FromAPITime( (long)RawData.api_mission[2] );
					break;

			}

		}


		public override void LoadFromRequest( string apiname, Dictionary<string, string> data ) {
			base.LoadFromRequest( apiname, data );


			switch ( apiname ) {
				case "api_req_hensei/change": {
						int fleetID = int.Parse( data["api_id"] );
						int index = int.Parse( data["api_ship_idx"] );
						int shipID = int.Parse( data["api_ship_id"] );
						int replacedID = data.ContainsKey( "replaced_id" ) ? int.Parse( data["replaced_id"] ) : -1;


						if ( FleetID == fleetID ) {
							if ( index == -1 ) {
								//旗艦以外全解除
								for ( int i = 1; i < _fleetMember.Length; i++ )
									_fleetMember[i] = -1;

							} else if ( shipID == -1 ) {
								//はずす
								RemoveShip( index );

							} else {
								//入隊

								//入れ替え
								for ( int i = 0; i < _fleetMember.Length; i++ ) {
									if ( _fleetMember[i] == shipID ) {
										_fleetMember[i] = replacedID;
										break;
									}
								}
								
								//入隊
								_fleetMember[index] = shipID;

							}


						} else {

							if ( index != -1 && shipID != -1 ) {
								//入れ替え
								for ( int i = 0; i < _fleetMember.Length; i++ ) {
									if ( _fleetMember[i] == shipID ) {
										_fleetMember[i] = replacedID;
										break;
									}
								}

							}

						}

					} break;


				case "api_req_kousyou/destroyship": {
						int shipID = int.Parse( data["api_ship_id"] );

						for ( int i = 0; i < _fleetMember.Length; i++ ) {
							if ( _fleetMember[i] == shipID ) {
								RemoveShip( i );
								break;
							}
						}
					} break;

				case "api_req_mission/start":
					ExpeditionState = 1;
					ExpeditionDestination = int.Parse( data["api_mission_id"] );
					ExpeditionTime = DateTime.Now;	//暫定処理。実際の更新はResponseで行う
					break;

			}

		}


		private void RemoveShip( int index ) {

			for ( int i = index + 1; i < _fleetMember.Length; i++ )
				_fleetMember[i - 1] = _fleetMember[i];

			_fleetMember[_fleetMember.Length - 1] = -1;

		}


	}

}
