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
  \file    robocup_ssl_client.h
  \brief   C++ Interface: robocup_ssl_client
  \author  Stefan Zickler, 2009
*/
//========================================================================
#ifndef ROBOCUP_SSL_CLIENT_M_H
#define ROBOCUP_SSL_CLIENT_M_H

//#include <windows.h>
#include <string>

#include "messages_robocup_ssl_wrapper.pb.h"
#include "messages_robocup_ssl_wrapper_m.pb.h"

#include "robocup_ssl_client.h"

using namespace std;
using namespace System::Runtime::InteropServices;
/**
	@author Author Name
*/

namespace SSLVision {
public __gc class RoboCupSSLClientManaged {
private:
	RoboCupSSLClient __nogc * m_pRoboCupSSLClient;
public:
	
	RoboCupSSLClientManaged() {
		m_pRoboCupSSLClient = new RoboCupSSLClient();
	}
    RoboCupSSLClientManaged(int port,
		System::String *net_ref_address,
		System::String *net_ref_interface) {
		char *str;
		str = (char*)(void*)Marshal::StringToHGlobalAnsi(net_ref_address);
		string net_ref_address_str(str);
		Marshal::FreeHGlobal(str);
		str = (char*)(void*)Marshal::StringToHGlobalAnsi(net_ref_interface);
		string net_ref_interface_str(str);
		Marshal::FreeHGlobal(str);
		
		m_pRoboCupSSLClient = new RoboCupSSLClient(port, net_ref_address_str, net_ref_interface_str);
	}

	
	~RoboCupSSLClientManaged() {
		delete m_pRoboCupSSLClient;
	}
	
	bool open() {
		return m_pRoboCupSSLClient->open();
	}
	bool open(bool blocking) {
		return m_pRoboCupSSLClient->open(blocking);
	}
	
	void close() {
		m_pRoboCupSSLClient->close();
	}
	
	bool receive(SSLVision::SSL_WrapperPacketManaged *packet) {
		::SSL_WrapperPacket &unmPacket = packet->unmanagedInstance();
		return m_pRoboCupSSLClient->receive(unmPacket);		
	}	
};
}
#endif
