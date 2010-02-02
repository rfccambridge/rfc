#ifndef MESSAGES_ROBOCUP_SSL_DETECTION_M_H
#define MESSAGES_ROBOCUP_SSL_DETECTION_M_H

// Regular expressions used to build the wrappers:
// Search: {(float|bool|int|double)} {(.#)}\({[^ ]@}{ @}{[^ ]@}\){( const)@};
// Replace: \1 \2(\3\4\5)\6 {\n      return m_pSSL_DetectionRobot->\2(\5);\n   }
// and
// Search: {void} {(.#)}\({[^ ]@}{ @}{[^ ]@}\){( const)@};
// Replace: \1 \2(\3\4\5)\6 {\n      m_pSSL_DetectionRobot->\2(\5);\n   }

#include "messages_robocup_ssl_detection.pb.h"

namespace SSLVision {
	public __gc class SSL_DetectionBallManaged {
	private:
		SSL_DetectionBall __nogc *m_pSSL_DetectionBall;		
	public:
		SSL_DetectionBallManaged() {
			m_pSSL_DetectionBall = new SSL_DetectionBall();			
		}
		SSL_DetectionBallManaged(const SSL_DetectionBall &from) {
			m_pSSL_DetectionBall = new SSL_DetectionBall(from);			
		}
		~SSL_DetectionBallManaged() {			
			delete m_pSSL_DetectionBall;
		}

		bool has_confidence() {
			return m_pSSL_DetectionBall->has_confidence();
		}
		void clear_confidence() {
			m_pSSL_DetectionBall->clear_confidence();
		}
		float confidence() {
			return m_pSSL_DetectionBall->confidence();
		}
		void set_confidence(float value) {
			m_pSSL_DetectionBall->set_confidence(value);
		}

		bool has_area() {
			return m_pSSL_DetectionBall->has_area();
		}
		void clear_area() {
			m_pSSL_DetectionBall->clear_area();
		}
		unsigned int area() {
			return m_pSSL_DetectionBall->area();
		}
		void set_area(unsigned int value) {
			return m_pSSL_DetectionBall->set_area(value);
		}

		bool has_x() {
			return m_pSSL_DetectionBall->has_x();
		}
		void clear_x() {
			m_pSSL_DetectionBall->clear_x();
		}
		float x() {
			return m_pSSL_DetectionBall->x();
		}
		void set_x(float value) {
			m_pSSL_DetectionBall->set_x(value);
		}

		bool has_y() {
			return m_pSSL_DetectionBall->has_y();
		}
		void clear_y() {
			m_pSSL_DetectionBall->clear_y();
		}
		float y() {
			return m_pSSL_DetectionBall->y();
		}
		void set_y(float value) {
			m_pSSL_DetectionBall->set_y(value);
		}

		bool has_z() {
			return m_pSSL_DetectionBall->has_z();
		}
		void clear_z() {
			m_pSSL_DetectionBall->clear_z();
		}
		float z() {
			return m_pSSL_DetectionBall->z();
		}
		void set_z(float value) {
			m_pSSL_DetectionBall->set_z(value);
		}

		bool has_pixel_x() {
			return m_pSSL_DetectionBall->has_pixel_x();
		}
		void clear_pixel_x() {
			m_pSSL_DetectionBall->clear_pixel_x();
		}
		float pixel_x() {
			return m_pSSL_DetectionBall->pixel_x();
		}
		void set_pixel_x(float value) {
			m_pSSL_DetectionBall->set_pixel_x(value);
		}

		bool has_pixel_y() {
			return m_pSSL_DetectionBall->has_pixel_y();
		}
		void clear_pixel_y() {
			m_pSSL_DetectionBall->clear_pixel_y();
		}
		float pixel_y() {
			return m_pSSL_DetectionBall->pixel_y();
		}
		void set_pixel_y(float value) {
			m_pSSL_DetectionBall->set_pixel_y(value);
		}
	};
	// -------------------------------------------------------------------

	public __gc class SSL_DetectionRobotManaged {
	private:
		SSL_DetectionRobot __nogc *m_pSSL_DetectionRobot;
	public:
		SSL_DetectionRobotManaged() {
			m_pSSL_DetectionRobot = new SSL_DetectionRobot();
		}
		SSL_DetectionRobotManaged(const SSL_DetectionRobot &from) {
			m_pSSL_DetectionRobot = new SSL_DetectionRobot(from);
		}
		~SSL_DetectionRobotManaged() {
			delete m_pSSL_DetectionRobot;
		}

		bool has_confidence() {
			return m_pSSL_DetectionRobot->has_confidence();
		}
		void clear_confidence() {
			m_pSSL_DetectionRobot->clear_confidence();
		}
		float confidence() {
			return m_pSSL_DetectionRobot->confidence();
		}
		void set_confidence(float value) {
			m_pSSL_DetectionRobot->set_confidence(value);
		}

		bool has_robot_id() {
			return m_pSSL_DetectionRobot->has_robot_id();
		}
		void clear_robot_id() {
			m_pSSL_DetectionRobot->clear_robot_id();
		}

		unsigned int robot_id() {
			return m_pSSL_DetectionRobot->robot_id();
		}

		void set_robot_id(unsigned int value) {
			m_pSSL_DetectionRobot->set_robot_id(value);
		}

		bool has_x() {
			return m_pSSL_DetectionRobot->has_x();
		}
		void clear_x() {
			m_pSSL_DetectionRobot->clear_x();
		}
		float x() {
			return m_pSSL_DetectionRobot->x();
		}
		void set_x(float value) {
			m_pSSL_DetectionRobot->set_x(value);
		}

		bool has_y() {
			return m_pSSL_DetectionRobot->has_y();
		}
		void clear_y() {
			m_pSSL_DetectionRobot->clear_y();
		}
		float y() {
			return m_pSSL_DetectionRobot->y();
		}
		void set_y(float value) {
			m_pSSL_DetectionRobot->set_y(value);
		}

		bool has_orientation() {
			return m_pSSL_DetectionRobot->has_orientation();
		}
		void clear_orientation() {
			m_pSSL_DetectionRobot->clear_orientation();
		}
		float orientation() {
			return m_pSSL_DetectionRobot->orientation();
		}
		void set_orientation(float value) {
			m_pSSL_DetectionRobot->set_orientation(value);
		}

		bool has_pixel_x() {
			return m_pSSL_DetectionRobot->has_pixel_x();
		}
		void clear_pixel_x() {
			m_pSSL_DetectionRobot->clear_pixel_x();
		}
		float pixel_x() {
			return m_pSSL_DetectionRobot->pixel_x();
		}
		void set_pixel_x(float value) {
			m_pSSL_DetectionRobot->set_pixel_x(value);
		}

		bool has_pixel_y() {
			return m_pSSL_DetectionRobot->has_pixel_y();
		}
		void clear_pixel_y() {
			m_pSSL_DetectionRobot->clear_pixel_y();
		}
		float pixel_y() {
			return m_pSSL_DetectionRobot->pixel_y();
		}
		void set_pixel_y(float value) {
			m_pSSL_DetectionRobot->set_pixel_y(value);
		}

		bool has_height() {
			return m_pSSL_DetectionRobot->has_height();
		}
		void clear_height() {
			m_pSSL_DetectionRobot->clear_height();
		}
		float height() {
			return m_pSSL_DetectionRobot->height();
		}
		void set_height(float value) {
			m_pSSL_DetectionRobot->set_height(value);
		}
	};
	// -------------------------------------------------------------------

	public __gc class SSL_DetectionFrameManaged {
	private:
		SSL_DetectionFrame __nogc *m_pSSL_DetectionFrame;
	public:
		SSL_DetectionFrameManaged() {
			m_pSSL_DetectionFrame = new SSL_DetectionFrame();
		}
		SSL_DetectionFrameManaged(const SSL_DetectionFrame &from) {
			m_pSSL_DetectionFrame = new SSL_DetectionFrame(from);
		}
		~SSL_DetectionFrameManaged() {
			delete m_pSSL_DetectionFrame;
		}

		::SSL_DetectionFrame &unmanagedInstance() {
			return *m_pSSL_DetectionFrame;
		}

		bool has_frame_number() {
			return m_pSSL_DetectionFrame->has_frame_number();
		}
		void clear_frame_number() {
			m_pSSL_DetectionFrame->clear_frame_number();
		}
		unsigned int frame_number() {
			return m_pSSL_DetectionFrame->frame_number();
		}
		void set_frame_number(unsigned int value) {
			m_pSSL_DetectionFrame->set_frame_number(value);
		}

		bool has_t_capture() {
			return m_pSSL_DetectionFrame->has_t_capture();
		}
		void clear_t_capture() {
			m_pSSL_DetectionFrame->clear_t_capture();
		}
		double t_capture() {
			return m_pSSL_DetectionFrame->t_capture();
		}
		void set_t_capture(double value) {
			m_pSSL_DetectionFrame->set_t_capture(value);
		}

		bool has_t_sent() {
			return m_pSSL_DetectionFrame->has_t_sent();
		}
		void clear_t_sent() {
			m_pSSL_DetectionFrame->clear_t_sent();
		}
		double t_sent() {
			return m_pSSL_DetectionFrame->t_sent();
		}
		void set_t_sent(double value) {
			m_pSSL_DetectionFrame->set_t_sent(value);
		}

		bool has_camera_id() {
			return m_pSSL_DetectionFrame->has_camera_id();
		}
		void clear_camera_id() {
			m_pSSL_DetectionFrame->clear_camera_id();
		}
		unsigned int camera_id() {
			return m_pSSL_DetectionFrame->camera_id();
		}
		void set_camera_id(unsigned int value) {
			m_pSSL_DetectionFrame->set_camera_id(value);
		}

		int balls_size() {
			return m_pSSL_DetectionFrame->balls_size();
		}
		void clear_balls() {
			m_pSSL_DetectionFrame->clear_balls();
		}   
		SSLVision::SSL_DetectionBallManaged *balls(int index) {
			const ::SSL_DetectionBall &detectionBall = m_pSSL_DetectionFrame->balls(index);
			return new SSLVision::SSL_DetectionBallManaged(detectionBall);
		}

		int robots_yellow_size() {
			return m_pSSL_DetectionFrame->robots_yellow_size();
		}
		void clear_robots_yellow() {
			m_pSSL_DetectionFrame->clear_robots_yellow();
		}

		SSLVision::SSL_DetectionRobotManaged *robots_yellow(int index) {
			const ::SSL_DetectionRobot &robot = m_pSSL_DetectionFrame->robots_yellow(index);
			return new SSLVision::SSL_DetectionRobotManaged(robot);
		}

		int robots_blue_size() {
			return m_pSSL_DetectionFrame->robots_blue_size();
		}
		void clear_robots_blue() {
			m_pSSL_DetectionFrame->clear_robots_blue();
		}   
		SSLVision::SSL_DetectionRobotManaged *robots_blue(int index) {
			const ::SSL_DetectionRobot &robot = m_pSSL_DetectionFrame->robots_blue(index);
			return new SSLVision::SSL_DetectionRobotManaged(robot);
		}
	};
}
#endif