#ifndef MESSAGES_ROBOCUP_SSL_WRAPPER_M_H
#define MESSAGES_ROBOCUP_SSL_WRAPPER_M_H

#include "messages_robocup_ssl_wrapper.pb.h"

#include "messages_robocup_ssl_geometry.pb.h"
#include "messages_robocup_ssl_geometry_m.pb.h"

#include "messages_robocup_ssl_detection.pb.h"
#include "messages_robocup_ssl_detection_m.pb.h"

namespace SSLVision {	

	public __gc class SSL_WrapperPacketManaged {
	private:
		SSL_WrapperPacket __nogc *m_pSSL_WrapperPacket;
	public:
		SSL_WrapperPacketManaged() {
			m_pSSL_WrapperPacket = new SSL_WrapperPacket();
		}
		SSL_WrapperPacketManaged(const SSL_WrapperPacket &from) {
			m_pSSL_WrapperPacket = new SSL_WrapperPacket(from);
		}
		~SSL_WrapperPacketManaged() {
			delete m_pSSL_WrapperPacket;
		}

		::SSL_WrapperPacket &unmanagedInstance() {
			return *m_pSSL_WrapperPacket;
		}

		bool has_detection() {
			return m_pSSL_WrapperPacket->has_detection();
		}
		void clear_detection() {
			m_pSSL_WrapperPacket->clear_detection();
		}
		SSLVision::SSL_DetectionFrameManaged *detection() {
			const ::SSL_DetectionFrame &frame = m_pSSL_WrapperPacket->detection();
			return new SSLVision::SSL_DetectionFrameManaged(frame);
		}
		
		bool has_geometry() {
			return m_pSSL_WrapperPacket->has_geometry();
		}
		void clear_geometry() {
			return m_pSSL_WrapperPacket->clear_geometry();
		}
		SSLVision::SSL_GeometryDataManaged *geometry() {
			const ::SSL_GeometryData &geometryData = m_pSSL_WrapperPacket->geometry();
			return new SSLVision::SSL_GeometryDataManaged(geometryData);
		}
		
		
	};
}
#endif 
