//========================================================================
//  This software is free: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License Version 3,
//  as published by the Free Software Foundation.
//
//  This software is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  Version 3 in the file COPYING that came with this distribution.
//  If not, see <http://www.gnu.org/licenses/>.
//========================================================================
/*!
  \file    robocup_ssl_server.h
  \brief   C++ Interface: robocup_ssl_server
  \author  Stefan Zickler, 2009
*/
//========================================================================
#ifndef ROBOCUP_SSL_SERVER_M_H
#define ROBOCUP_SSL_SERVER_M_H

#include <string>
//#include <QMutex>
#include "messages_robocup_ssl_detection.pb.h"
#include "messages_robocup_ssl_geometry.pb.h"
#include "messages_robocup_ssl_wrapper.pb.h"

#include "messages_robocup_ssl_detection_m.pb.h"
#include "messages_robocup_ssl_geometry_m.pb.h"
#include "messages_robocup_ssl_wrapper_m.pb.h"

#include "robocup_ssl_server.h"

using namespace std;
using namespace System::Runtime::InteropServices;
/**
	@author Stefan Zickler
*/

namespace SSLVision {
public __gc class RoboCupSSLServerManaged {
private:
	RoboCupSSLServer __nogc * m_pRoboCupSSLServer;
public:
	RoboCupSSLServerManaged(int port,
		System::String *net_ref_address,
		System::String *net_ref_interface) {
		char *str;
		str = (char*)(void*)Marshal::StringToHGlobalAnsi(net_ref_address);
		string net_ref_address_str(str);
		Marshal::FreeHGlobal(str);
		str = (char*)(void*)Marshal::StringToHGlobalAnsi(net_ref_interface);
		string net_ref_interface_str(str);
		Marshal::FreeHGlobal(str);
		
		m_pRoboCupSSLServer = new RoboCupSSLServer(port, net_ref_address_str, net_ref_interface_str);
	}

	~RoboCupSSLServerManaged() {
		delete m_pRoboCupSSLServer;
	}
	bool open() {
		return m_pRoboCupSSLServer->open();
	}
	void close() {
		m_pRoboCupSSLServer->close();
	}
	bool send(SSL_WrapperPacketManaged * packet) {
		::SSL_WrapperPacket &unmPacket = packet->unmanagedInstance();
		return m_pRoboCupSSLServer->send(unmPacket);
	}
	bool send(SSL_DetectionFrameManaged * frame) {
		::SSL_DetectionFrame &unmFrame = frame->unmanagedInstance();
		return m_pRoboCupSSLServer->send(unmFrame);
	}
	bool send(SSL_GeometryDataManaged * geometry) {
		::SSL_GeometryData &unmGeomData = geometry->unmanagedInstance();
		return m_pRoboCupSSLServer->send(unmGeomData);
	}

};
}

#endif
