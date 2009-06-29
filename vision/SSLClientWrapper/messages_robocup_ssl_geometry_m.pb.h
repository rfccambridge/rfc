#ifndef MESSAGES_ROBOCUP_SSL_GEOMETRY_M_H
#define MESSAGES_ROBOCUP_SSL_GEOMETRY_M_H

#include "messages_robocup_ssl_geometry.pb.h"
#include "messages_robocup_ssl_geometry_m.pb.h"

// ===================================================================

namespace SSLVision {
	public __gc class SSL_GeometryFieldSizeManaged {
	private:
		SSL_GeometryFieldSize __nogc *m_pSSL_GeometryFieldSize;
	public:
		SSL_GeometryFieldSizeManaged() {
			m_pSSL_GeometryFieldSize = new SSL_GeometryFieldSize();
		}
		SSL_GeometryFieldSizeManaged(const SSL_GeometryFieldSize& from) {
			m_pSSL_GeometryFieldSize = new SSL_GeometryFieldSize(from);
		}
		~SSL_GeometryFieldSizeManaged() {
			delete m_pSSL_GeometryFieldSize;
		} 

		bool has_line_width() {
			return m_pSSL_GeometryFieldSize->has_line_width();
		}
		void clear_line_width() {
			m_pSSL_GeometryFieldSize->clear_line_width();
		}
		int line_width() {
			return m_pSSL_GeometryFieldSize->line_width();
		}
		void set_line_width(int value) {
			m_pSSL_GeometryFieldSize->set_line_width(value);
		}

		bool has_field_length() {
			return m_pSSL_GeometryFieldSize->has_field_length();
		}
		void clear_field_length() {
			m_pSSL_GeometryFieldSize->clear_field_length();
		}
		int field_length() {
			return m_pSSL_GeometryFieldSize->field_length();
		}
		void set_field_length(int value) {
			m_pSSL_GeometryFieldSize->set_field_length(value);
		}

		bool has_field_width() {
			return m_pSSL_GeometryFieldSize->has_field_width();
		}
		void clear_field_width() {
			m_pSSL_GeometryFieldSize->clear_field_width();
		}
		int field_width() {
			return m_pSSL_GeometryFieldSize->field_width();
		}
		void set_field_width(int value) {
			m_pSSL_GeometryFieldSize->set_field_width(value);
		}

		bool has_boundary_width() {
			return m_pSSL_GeometryFieldSize->has_boundary_width();
		}
		void clear_boundary_width() {
			m_pSSL_GeometryFieldSize->clear_boundary_width();
		}
		int boundary_width() {
			return m_pSSL_GeometryFieldSize->boundary_width();
		}
		void set_boundary_width(int value) {
			m_pSSL_GeometryFieldSize->set_boundary_width(value);
		}

		bool has_referee_width() {
			return m_pSSL_GeometryFieldSize->has_referee_width();
		}
		void clear_referee_width() {
			m_pSSL_GeometryFieldSize->clear_referee_width();
		}
		int referee_width() {
			return m_pSSL_GeometryFieldSize->referee_width();
		}
		void set_referee_width(int value) {
			m_pSSL_GeometryFieldSize->set_referee_width(value);
		}

		bool has_goal_width() {
			return m_pSSL_GeometryFieldSize->has_goal_width();
		}
		void clear_goal_width() {
			m_pSSL_GeometryFieldSize->clear_goal_width();
		}
		int goal_width() {
			return m_pSSL_GeometryFieldSize->goal_width();
		}
		void set_goal_width(int value) {
			m_pSSL_GeometryFieldSize->set_goal_width(value);
		}

		bool has_goal_depth() {
			return m_pSSL_GeometryFieldSize->has_goal_depth();
		}
		void clear_goal_depth() {
			m_pSSL_GeometryFieldSize->clear_goal_depth();
		}
		int goal_depth() {
			return m_pSSL_GeometryFieldSize->goal_depth();
		}
		void set_goal_depth(int value) {
			m_pSSL_GeometryFieldSize->set_goal_depth(value);
		}

		bool has_goal_wall_width() {
			return m_pSSL_GeometryFieldSize->has_goal_wall_width();
		}
		void clear_goal_wall_width() {
			m_pSSL_GeometryFieldSize->clear_goal_wall_width();
		}
		int goal_wall_width() {
			return m_pSSL_GeometryFieldSize->goal_wall_width();
		}
		void set_goal_wall_width(int value) {
			m_pSSL_GeometryFieldSize->set_goal_wall_width(value);
		}

		bool has_center_circle_radius() {
			return m_pSSL_GeometryFieldSize->has_center_circle_radius();
		}
		void clear_center_circle_radius() {
			m_pSSL_GeometryFieldSize->clear_center_circle_radius();
		}
		int center_circle_radius() {
			return m_pSSL_GeometryFieldSize->center_circle_radius();
		}
		void set_center_circle_radius(int value) {
			m_pSSL_GeometryFieldSize->set_center_circle_radius(value);
		}

		bool has_defense_radius() {
			return m_pSSL_GeometryFieldSize->has_defense_radius();
		}
		void clear_defense_radius() {
			m_pSSL_GeometryFieldSize->clear_defense_radius();
		}
		int defense_radius() {
			return m_pSSL_GeometryFieldSize->defense_radius();
		}
		void set_defense_radius(int value) {
			m_pSSL_GeometryFieldSize->set_defense_radius(value);
		}

		bool has_defense_stretch() {
			return m_pSSL_GeometryFieldSize->has_defense_stretch();
		}
		void clear_defense_stretch() {
			m_pSSL_GeometryFieldSize->clear_defense_stretch();
		}
		int defense_stretch() {
			return m_pSSL_GeometryFieldSize->defense_stretch();
		}
		void set_defense_stretch(int value) {
			m_pSSL_GeometryFieldSize->set_defense_stretch(value);
		}

		bool has_free_kick_from_defense_dist() {
			return m_pSSL_GeometryFieldSize->has_free_kick_from_defense_dist();
		}
		void clear_free_kick_from_defense_dist() {
			m_pSSL_GeometryFieldSize->clear_free_kick_from_defense_dist();
		}
		int free_kick_from_defense_dist() {
			return m_pSSL_GeometryFieldSize->free_kick_from_defense_dist();
		}
		void set_free_kick_from_defense_dist(int value) {
			m_pSSL_GeometryFieldSize->set_free_kick_from_defense_dist(value);
		}

		bool has_penalty_spot_from_field_line_dist() {
			return m_pSSL_GeometryFieldSize->has_penalty_spot_from_field_line_dist();
		}
		void clear_penalty_spot_from_field_line_dist() {
			m_pSSL_GeometryFieldSize->clear_penalty_spot_from_field_line_dist();
		}
		int penalty_spot_from_field_line_dist() {
			return m_pSSL_GeometryFieldSize->penalty_spot_from_field_line_dist();
		}
		void set_penalty_spot_from_field_line_dist(int value) {
			m_pSSL_GeometryFieldSize->set_penalty_spot_from_field_line_dist(value);
		}

		bool has_penalty_line_from_spot_dist() {
			return m_pSSL_GeometryFieldSize->has_penalty_line_from_spot_dist();
		}
		void clear_penalty_line_from_spot_dist() {
			m_pSSL_GeometryFieldSize->clear_penalty_line_from_spot_dist();
		}
		int penalty_line_from_spot_dist() {
			return m_pSSL_GeometryFieldSize->penalty_line_from_spot_dist();
		}
		void set_penalty_line_from_spot_dist(int value) {
			m_pSSL_GeometryFieldSize->set_penalty_line_from_spot_dist(value);
		}


	};
	// -------------------------------------------------------------------

	public __gc class SSL_GeometryCameraCalibrationManaged {
	private:
		SSL_GeometryCameraCalibration __nogc *m_pSSL_GeometryCameraCalibration;
	public:
		SSL_GeometryCameraCalibrationManaged() {
			m_pSSL_GeometryCameraCalibration = new SSL_GeometryCameraCalibration();
		}
		SSL_GeometryCameraCalibrationManaged(const SSL_GeometryCameraCalibration& from) {
			m_pSSL_GeometryCameraCalibration = new SSL_GeometryCameraCalibration(from);
		}
		~SSL_GeometryCameraCalibrationManaged() {
			delete m_pSSL_GeometryCameraCalibration;
		}

		// required uint32 camera_id = 1;
		bool has_camera_id() {
			return m_pSSL_GeometryCameraCalibration->has_camera_id();
		}
		void clear_camera_id() {
			m_pSSL_GeometryCameraCalibration->clear_camera_id();
		}
		unsigned int camera_id() {
			return m_pSSL_GeometryCameraCalibration->camera_id();
		}
		void set_camera_id(unsigned int value) {
			m_pSSL_GeometryCameraCalibration->set_camera_id(value);
		}

		// required float focal_length = 2;
		bool has_focal_length() {
			return m_pSSL_GeometryCameraCalibration->has_focal_length();
		}
		void clear_focal_length() {
			m_pSSL_GeometryCameraCalibration->clear_focal_length();
		}
		float focal_length() {
			return m_pSSL_GeometryCameraCalibration->focal_length();
		}
		void set_focal_length(float value) {
			m_pSSL_GeometryCameraCalibration->set_focal_length(value);
		}

		// required float principal_point_x = 3;
		bool has_principal_point_x() {
			return m_pSSL_GeometryCameraCalibration->has_principal_point_x();
		}
		void clear_principal_point_x() {
			m_pSSL_GeometryCameraCalibration->clear_principal_point_x();
		}
		float principal_point_x() {
			return m_pSSL_GeometryCameraCalibration->principal_point_x();
		}
		void set_principal_point_x(float value) {
			m_pSSL_GeometryCameraCalibration->set_principal_point_x(value);
		}

		// required float principal_point_y = 4;
		bool has_principal_point_y() {
			return m_pSSL_GeometryCameraCalibration->has_principal_point_y();
		}
		void clear_principal_point_y() {
			m_pSSL_GeometryCameraCalibration->clear_principal_point_y();
		}
		float principal_point_y() {
			return m_pSSL_GeometryCameraCalibration->principal_point_y();
		}
		void set_principal_point_y(float value) {
			m_pSSL_GeometryCameraCalibration->set_principal_point_y(value);
		}

		// required float distortion = 5;
		bool has_distortion() {
			return m_pSSL_GeometryCameraCalibration->has_distortion();
		}
		void clear_distortion() {
			m_pSSL_GeometryCameraCalibration->clear_distortion();
		}
		float distortion() {
			return m_pSSL_GeometryCameraCalibration->distortion();
		}
		void set_distortion(float value) {
			m_pSSL_GeometryCameraCalibration->set_distortion(value);
		}

		// required float q0 = 6;
		bool has_q0() {
			return m_pSSL_GeometryCameraCalibration->has_q0();
		}
		void clear_q0() {
			m_pSSL_GeometryCameraCalibration->clear_q0();
		}
		float q0() {
			return m_pSSL_GeometryCameraCalibration->q0();
		}
		void set_q0(float value) {
			m_pSSL_GeometryCameraCalibration->set_q0(value);
		}

		// required float q1 = 7;
		bool has_q1() {
			return m_pSSL_GeometryCameraCalibration->has_q1();
		}
		void clear_q1() {
			m_pSSL_GeometryCameraCalibration->clear_q1();
		}
		float q1() {
			return m_pSSL_GeometryCameraCalibration->q1();
		}
		void set_q1(float value) {
			m_pSSL_GeometryCameraCalibration->set_q1(value);
		}

		// required float q2 = 8;
		bool has_q2() {
			return m_pSSL_GeometryCameraCalibration->has_q2();
		}
		void clear_q2() {
			m_pSSL_GeometryCameraCalibration->clear_q2();
		}
		float q2() {
			return m_pSSL_GeometryCameraCalibration->q2();
		}
		void set_q2(float value) {
			m_pSSL_GeometryCameraCalibration->set_q2(value);
		}

		// required float q3 = 9;
		bool has_q3() {
			return m_pSSL_GeometryCameraCalibration->has_q3();
		}
		void clear_q3() {
			m_pSSL_GeometryCameraCalibration->clear_q3();
		}
		float q3() {
			return m_pSSL_GeometryCameraCalibration->q3();
		}
		void set_q3(float value) {
			m_pSSL_GeometryCameraCalibration->set_q3(value);
		}

		// required float tx = 10;
		bool has_tx() {
			return m_pSSL_GeometryCameraCalibration->has_tx();
		}
		void clear_tx() {
			m_pSSL_GeometryCameraCalibration->clear_tx();
		}
		float tx() {
			return m_pSSL_GeometryCameraCalibration->tx();
		}
		void set_tx(float value) {
			m_pSSL_GeometryCameraCalibration->set_tx(value);
		}

		// required float ty = 11;
		bool has_ty() {
			return m_pSSL_GeometryCameraCalibration->has_ty();
		}
		void clear_ty() {
			m_pSSL_GeometryCameraCalibration->clear_ty();
		}
		float ty() {
			return m_pSSL_GeometryCameraCalibration->ty();
		}
		void set_ty(float value) {
			m_pSSL_GeometryCameraCalibration->set_ty(value);
		}

		// required float tz = 12;
		bool has_tz() {
			return m_pSSL_GeometryCameraCalibration->has_tz();
		}
		void clear_tz() {
			m_pSSL_GeometryCameraCalibration->clear_tz();
		}
		float tz() {
			return m_pSSL_GeometryCameraCalibration->tz();
		}
		void set_tz(float value) {
			m_pSSL_GeometryCameraCalibration->set_tz(value);
		}

		// optional float derived_camera_world_tx = 13;
		bool has_derived_camera_world_tx() {
			return m_pSSL_GeometryCameraCalibration->has_derived_camera_world_tx();
		}
		void clear_derived_camera_world_tx() {
			m_pSSL_GeometryCameraCalibration->clear_derived_camera_world_tx();
		}
		float derived_camera_world_tx() {
			return m_pSSL_GeometryCameraCalibration->derived_camera_world_tx();
		}
		void set_derived_camera_world_tx(float value) {
			m_pSSL_GeometryCameraCalibration->set_derived_camera_world_tx(value);
		}

		// optional float derived_camera_world_ty = 14;
		bool has_derived_camera_world_ty() {
			return m_pSSL_GeometryCameraCalibration->has_derived_camera_world_ty();
		}
		void clear_derived_camera_world_ty() {
			m_pSSL_GeometryCameraCalibration->clear_derived_camera_world_ty();
		}
		float derived_camera_world_ty() {
			return m_pSSL_GeometryCameraCalibration->derived_camera_world_ty();
		}
		void set_derived_camera_world_ty(float value) {
			m_pSSL_GeometryCameraCalibration->set_derived_camera_world_ty(value);
		}

		// optional float derived_camera_world_tz = 15;
		bool has_derived_camera_world_tz() {
			return m_pSSL_GeometryCameraCalibration->has_derived_camera_world_tz();
		}
		void clear_derived_camera_world_tz() {
			m_pSSL_GeometryCameraCalibration->clear_derived_camera_world_tz();
		}
		float derived_camera_world_tz() {
			return m_pSSL_GeometryCameraCalibration->derived_camera_world_tz();
		}
		void set_derived_camera_world_tz(float value) {
			m_pSSL_GeometryCameraCalibration->set_derived_camera_world_tz(value);
		}
	};
	// -------------------------------------------------------------------


	public __gc class SSL_GeometryDataManaged {
	private:
		SSL_GeometryData __nogc *m_pSSL_GeometryData;
	public:
		SSL_GeometryDataManaged() {
			m_pSSL_GeometryData = new SSL_GeometryData();
		}
		SSL_GeometryDataManaged(const SSL_GeometryData& from) {
			m_pSSL_GeometryData = new SSL_GeometryData(from);
		}
		~SSL_GeometryDataManaged() {
			delete m_pSSL_GeometryData;
		}

		bool has_field() {
			return m_pSSL_GeometryData->has_field();
		}
		void clear_field() {
			m_pSSL_GeometryData->clear_field();
		}		
		SSLVision::SSL_GeometryFieldSizeManaged *field() {
			const ::SSL_GeometryFieldSize &fieldSize = m_pSSL_GeometryData->field();
			return new SSLVision::SSL_GeometryFieldSizeManaged(fieldSize);
		}			
		
		int calib_size() {
			return m_pSSL_GeometryData->calib_size();
		}
		void clear_calib() {
			m_pSSL_GeometryData->clear_calib();
		}
		SSLVision::SSL_GeometryCameraCalibrationManaged *calib(int index) {
			const ::SSL_GeometryCameraCalibration &calib = m_pSSL_GeometryData->calib(index);
			return new SSLVision::SSL_GeometryCameraCalibrationManaged(calib);
		}
		
	};
}
#endif